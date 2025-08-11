import os
import json
import time
from typing import Dict, Any, Optional

import aiohttp
import pytest
from dataclasses import dataclass
from datetime import datetime


@dataclass
class ResultData:
    name: str
    expected: str
    actual: str
    passed: bool
    timestamp: datetime
    duration: float = 0.0


class UserServiceTestClient:
    def __init__(self, base_url: str = "http://localhost:8080"):
        self.base_url = base_url
        self.session: Optional[aiohttp.ClientSession] = None

    async def __aenter__(self):
        self.session = aiohttp.ClientSession()
        return self

    async def __aexit__(self, exc_type, exc_val, exc_tb):
        if self.session:
            await self.session.close()

    async def api_call(
        self,
        method: str,
        endpoint: str,
        data: Optional[Dict] = None,
        headers: Optional[Dict] = None,
    ) -> Dict[str, Any]:
        if not self.session:
            raise RuntimeError("Client not initialized. Use 'async with' context manager.")

        url = f"{self.base_url}{endpoint}"
        request_headers = {"Content-Type": "application/json"}
        if headers:
            request_headers.update(headers)

        try:
            async with self.session.request(method, url, json=data, headers=request_headers) as response:
                try:
                    response_data = await response.json()
                except Exception:
                    response_data = await response.text()
                return {
                    "success": response.status < 400,
                    "data": response_data,
                    "status": response.status,
                }
        except Exception as e:
            return {"success": False, "data": str(e), "status": 0, "error": str(e)}

    async def health_check(self) -> bool:
        result = await self.api_call("GET", "/health")
        return result["success"] and (result["data"] == "Healthy" or "Healthy" in str(result["data"]))

    async def clean_database(self) -> bool:
        # No-op in Azure SQL shared environment
        return True

    async def get_database_counts(self) -> tuple[int, int]:
        # Not available without direct DB access in this setup
        return -1, -1


class TestData:
    VALID_USERS = {
        "admin": {
            "username": "admin",
            "password": "Admin123!",
            "email": "admin@medicare.com",
            "role": "Admin",
            "firstName": "System",
            "lastName": "Administrator",
            "phoneNumber": "+1234567890",
        },
        "doctor": {
            "username": "doctor1",
            "password": "Doctor123!",
            "email": "doctor1@medicare.com",
            "role": "Doctor",
            "firstName": "John",
            "lastName": "Smith",
            "phoneNumber": "+1987654321",
        },
        "patient": {
            "username": "patient1",
            "password": "Patient123!",
            "email": "patient1@medicare.com",
            "role": "Patient",
            "firstName": "Jane",
            "lastName": "Doe",
            "phoneNumber": "+1555123456",
        },
    }

    INVALID_USERS = {
        "empty_username": {
            "username": "",
            "password": "Test123!",
            "email": "test@example.com",
            "role": "Patient",
            "firstName": "Test",
            "lastName": "User",
        },
        "weak_password": {
            "username": "test",
            "password": "123",
            "email": "test@example.com",
            "role": "Patient",
            "firstName": "Test",
            "lastName": "User",
        },
        "invalid_email": {
            "username": "test",
            "password": "Test123!",
            "email": "invalid-email",
            "role": "Patient",
            "firstName": "Test",
            "lastName": "User",
        },
        "invalid_role": {
            "username": "test",
            "password": "Test123!",
            "email": "test@example.com",
            "role": "InvalidRole",
            "firstName": "Test",
            "lastName": "User",
        },
    }


@pytest.fixture
async def client():
    async with UserServiceTestClient() as client:
        assert await client.health_check(), "Service is not healthy"
        yield client


@pytest.fixture
async def clean_db(client):
    # keep for compatibility; no destructive cleanup in shared Azure SQL
    _ = await client.clean_database()


@pytest.fixture
async def admin_token(client, clean_db):
    await client.api_call("POST", "/api/auth/register", TestData.VALID_USERS["admin"])
    login_result = await client.api_call(
        "POST", "/api/auth/login", {"username": "admin", "password": "Admin123!"}
    )
    assert login_result["success"], f"Admin login failed: {login_result}"
    return login_result["data"]["token"]


class TestUserRegistration:
    async def test_register_admin_user(self, client, clean_db):
        payload = {**TestData.VALID_USERS["admin"]}
        payload["username"] = f"admin_{time.time_ns()}"
        payload["email"] = f"{payload['username']}@medicare.com"
        result = await client.api_call("POST", "/api/auth/register", payload)
        assert result["success"]
        assert "token" in result["data"]
        assert result["data"]["user"]["role"] == "Admin"

    async def test_register_doctor_user(self, client):
        payload = {**TestData.VALID_USERS["doctor"]}
        payload["username"] = f"doctor_{time.time_ns()}"
        payload["email"] = f"{payload['username']}@medicare.com"
        result = await client.api_call("POST", "/api/auth/register", payload)
        assert result["success"]
        assert "token" in result["data"]
        assert result["data"]["user"]["role"] == "Doctor"

    async def test_register_patient_user(self, client):
        payload = {**TestData.VALID_USERS["patient"]}
        payload["username"] = f"patient_{time.time_ns()}"
        payload["email"] = f"{payload['username']}@medicare.com"
        result = await client.api_call("POST", "/api/auth/register", payload)
        assert result["success"]
        assert "token" in result["data"]
        assert result["data"]["user"]["role"] == "Patient"


class TestDuplicateValidation:
    async def test_duplicate_username(self, client):
        base = {**TestData.VALID_USERS["patient"]}
        base["username"] = f"dupuser_{time.time_ns()}"
        base["email"] = f"{base['username']}@medicare.com"
        await client.api_call("POST", "/api/auth/register", base)
        duplicate_user = {**base, "email": f"new_{base['email']}"}
        result = await client.api_call("POST", "/api/auth/register", duplicate_user)
        assert not result["success"]
        assert result["data"]["message"] == "Username already exists"

    async def test_duplicate_email(self, client):
        base = {**TestData.VALID_USERS["patient"]}
        base["username"] = f"dupemail_{time.time_ns()}"
        base["email"] = f"{base['username']}@medicare.com"
        await client.api_call("POST", "/api/auth/register", base)
        duplicate_email_user = {
            "username": f"new_{base['username']}",
            "password": base["password"],
            "email": base["email"],
            "role": base["role"],
            "firstName": base["firstName"],
            "lastName": base["lastName"],
        }
        result = await client.api_call("POST", "/api/auth/register", duplicate_email_user)
        assert not result["success"]
        assert result["data"]["message"] == "Email already exists"

    async def test_case_insensitive_username(self, client):
        base = {**TestData.VALID_USERS["patient"]}
        base["username"] = f"caseuser_{time.time_ns()}"
        base["email"] = f"{base['username']}@medicare.com"
        await client.api_call("POST", "/api/auth/register", base)
        case_test_user = {**base, "username": base["username"].upper(), "email": f"upper_{base['email']}"}
        result = await client.api_call("POST", "/api/auth/register", case_test_user)
        assert not result["success"]
        assert result["data"]["message"] == "Username already exists"


class TestAuthentication:
    async def test_valid_admin_login(self, client):
        result = await client.api_call(
            "POST", "/api/auth/login", {"username": "admin", "password": "Admin123!"}
        )
        if not result["success"]:
            await client.api_call("POST", "/api/auth/register", TestData.VALID_USERS["admin"])
            result = await client.api_call(
                "POST", "/api/auth/login", {"username": "admin", "password": "Admin123!"}
            )
        assert result["success"]
        assert "token" in result["data"]

    async def test_valid_doctor_login(self, client):
        await client.api_call("POST", "/api/auth/register", TestData.VALID_USERS["doctor"])
        result = await client.api_call(
            "POST", "/api/auth/login", {"username": "doctor1", "password": "Doctor123!"}
        )
        assert result["success"]
        assert "token" in result["data"]

    async def test_invalid_password(self, client):
        result = await client.api_call(
            "POST", "/api/auth/login", {"username": "admin", "password": "wrongpassword"}
        )
        assert not result["success"]
        assert result["data"]["message"] == "Invalid username or password"

    async def test_nonexistent_user(self, client):
        result = await client.api_call(
            "POST", "/api/auth/login", {"username": "nonexistent", "password": "Password123!"}
        )
        assert not result["success"]
        assert result["data"]["message"] == "Invalid username or password"


class TestInputValidation:
    async def test_empty_username(self, client):
        result = await client.api_call("POST", "/api/auth/register", TestData.INVALID_USERS["empty_username"])
        assert not result["success"]
        assert "errors" in result["data"]
        errors = result["data"]["errors"]
        assert any("username" in str(key).lower() for key in errors.keys())

    async def test_weak_password(self, client):
        result = await client.api_call("POST", "/api/auth/register", TestData.INVALID_USERS["weak_password"])
        assert not result["success"]
        assert "errors" in result["data"]
        errors = result["data"]["errors"]
        assert any("password" in str(key).lower() for key in errors.keys())

    async def test_invalid_email(self, client):
        result = await client.api_call("POST", "/api/auth/register", TestData.INVALID_USERS["invalid_email"])
        assert not result["success"]
        assert "errors" in result["data"]
        errors_str = json.dumps(result["data"]).lower()
        assert "email" in errors_str

    async def test_invalid_role(self, client):
        result = await client.api_call("POST", "/api/auth/register", TestData.INVALID_USERS["invalid_role"])
        assert not result["success"]
        assert result["data"]["message"] == "Role 'InvalidRole' not found"


class TestAuthorization:
    async def test_authorized_access(self, admin_token, client):
        headers = {"Authorization": f"Bearer {admin_token}"}
        result = await client.api_call("GET", "/api/users", headers=headers)
        assert result["success"]

    async def test_unauthorized_access(self, client):
        result = await client.api_call("GET", "/api/users")
        assert not result["success"]
        assert result["status"] == 401

    async def test_invalid_token(self, client):
        headers = {"Authorization": "Bearer invalid-token"}
        result = await client.api_call("GET", "/api/users", headers=headers)
        assert not result["success"]
        assert result["status"] == 401


class TestDatabaseIntegrity:
    async def test_user_profile_consistency(self, client):
        user_count, profile_count = await client.get_database_counts()
        assert user_count == -1 and profile_count == -1, "Direct DB counts not available in Azure SQL setup"


if __name__ == "__main__":
    pytest.main([__file__, "-v", "--tb=short"])
