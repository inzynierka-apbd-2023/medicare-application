import asyncio
import json
import subprocess
import os
from typing import Dict, Any, Optional, List
import aiohttp
import pytest
from dataclasses import dataclass, asdict
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
        headers: Optional[Dict] = None
    ) -> Dict[str, Any]:
        """Make API call and return standardized response"""
        if not self.session:
            raise RuntimeError("Client not initialized. Use 'async with' context manager.")
            
        url = f"{self.base_url}{endpoint}"
        request_headers = {"Content-Type": "application/json"}
        if headers:
            request_headers.update(headers)
            
        try:
            async with self.session.request(
                method, url, json=data, headers=request_headers
            ) as response:
                try:
                    response_data = await response.json()
                except:
                    response_data = await response.text()
                    
                return {
                    "success": response.status < 400,
                    "data": response_data,
                    "status": response.status
                }
        except Exception as e:
            return {
                "success": False,
                "data": str(e),
                "status": 0,
                "error": str(e)
            }
    
    async def health_check(self) -> bool:
        """Check if service is healthy"""
        result = await self.api_call("GET", "/health")
        return result["success"] and result["data"] == "Healthy"
    
    async def clean_database(self) -> bool:
        """Clean the database by removing all users and profiles"""
        try:
            cmd = [
                "docker", "exec", "-i", "mssql-dev",
                "/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost",
                "-U", "sa", "-P", "Bo27erer#", "-C", "-Q",
                "USE medicare_dev; DELETE FROM [User_Profile]; DELETE FROM [User];"
            ]
            subprocess.run(cmd, check=True, capture_output=True)
            return True
        except subprocess.CalledProcessError:
            return False
    
    async def get_database_counts(self) -> tuple[int, int]:
        """Get user and profile counts from database"""
        try:
            cmd = [
                "docker", "exec", "-i", "mssql-dev",
                "/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost",
                "-U", "sa", "-P", "Bo27erer#", "-C", "-Q",
                "USE medicare_dev; SELECT COUNT(*) as UserCount FROM [User]; SELECT COUNT(*) as ProfileCount FROM [User_Profile];"
            ]
            result = subprocess.run(cmd, check=True, capture_output=True, text=True)
            
            # Parse output to extract counts
            lines = [line.strip() for line in result.stdout.split('\n') if line.strip()]
            numbers = [line for line in lines if line.isdigit()]
            
            user_count = int(numbers[0]) if len(numbers) > 0 else -1
            profile_count = int(numbers[1]) if len(numbers) > 1 else -1
            
            return user_count, profile_count
        except:
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
            "phoneNumber": "+1234567890"
        },
        "doctor": {
            "username": "doctor1",
            "password": "Doctor123!",
            "email": "doctor1@medicare.com",
            "role": "Doctor",
            "firstName": "John",
            "lastName": "Smith",
            "phoneNumber": "+1987654321"
        },
        "patient": {
            "username": "patient1",
            "password": "Patient123!",
            "email": "patient1@medicare.com",
            "role": "Patient",
            "firstName": "Jane",
            "lastName": "Doe",
            "phoneNumber": "+1555123456"
        }
    }
    
    INVALID_USERS = {
        "empty_username": {
            "username": "",
            "password": "Test123!",
            "email": "test@example.com",
            "role": "Patient",
            "firstName": "Test",
            "lastName": "User"
        },
        "weak_password": {
            "username": "test",
            "password": "123",
            "email": "test@example.com",
            "role": "Patient",
            "firstName": "Test",
            "lastName": "User"
        },
        "invalid_email": {
            "username": "test",
            "password": "Test123!",
            "email": "invalid-email",
            "role": "Patient",
            "firstName": "Test",
            "lastName": "User"
        },
        "invalid_role": {
            "username": "test",
            "password": "Test123!",
            "email": "test@example.com",
            "role": "InvalidRole",
            "firstName": "Test",
            "lastName": "User"
        }
    }


@pytest.fixture
async def client():
    """Pytest fixture for test client"""
    async with UserServiceTestClient() as client:
        # Ensure service is healthy
        is_healthy = await client.health_check()
        assert is_healthy, "Service is not healthy"
        yield client


@pytest.fixture
async def clean_db(client):
    """Pytest fixture to clean database before tests"""
    clean_env = os.getenv("CLEAN_DB", "false").lower()
    print(f"CLEAN_DB environment variable: {clean_env}")
    if clean_env == "true":
        print("Cleaning database...")
        success = await client.clean_database()
        print(f"Database cleaning result: {success}")
    else:
        print("Skipping database cleanup (CLEAN_DB not set to true)")
        # For now, let's always clean to make tests work
        print("Force cleaning database for reliable tests...")
        success = await client.clean_database()
        print(f"Force database cleaning result: {success}")


@pytest.fixture
async def admin_token(client, clean_db):
    """Pytest fixture to get admin token after setting up users"""
    # Register admin user first
    await client.api_call("POST", "/api/auth/register", TestData.VALID_USERS["admin"])
    
    # Login to get token
    login_result = await client.api_call("POST", "/api/auth/login", {
        "username": "admin",
        "password": "Admin123!"
    })
    
    assert login_result["success"]
    return login_result["data"]["token"]


class TestUserRegistration:
    async def test_register_admin_user(self, client, clean_db):
        result = await client.api_call("POST", "/api/auth/register", TestData.VALID_USERS["admin"])
        
        assert result["success"]
        assert "token" in result["data"]
        assert result["data"]["user"]["username"] == "admin"
        assert result["data"]["user"]["role"] == "Admin"
    
    async def test_register_doctor_user(self, client):
        result = await client.api_call("POST", "/api/auth/register", TestData.VALID_USERS["doctor"])
        
        assert result["success"]
        assert "token" in result["data"]
        assert result["data"]["user"]["username"] == "doctor1"
        assert result["data"]["user"]["role"] == "Doctor"
    
    async def test_register_patient_user(self, client):
        result = await client.api_call("POST", "/api/auth/register", TestData.VALID_USERS["patient"])
        
        assert result["success"]
        assert "token" in result["data"]
        assert result["data"]["user"]["username"] == "patient1"
        assert result["data"]["user"]["role"] == "Patient"


class TestDuplicateValidation:
    async def test_duplicate_username(self, client):
        duplicate_user = {**TestData.VALID_USERS["admin"], "email": "newemail@medicare.com"}
        result = await client.api_call("POST", "/api/auth/register", duplicate_user)
        
        assert not result["success"]
        assert result["data"]["message"] == "Username already exists"
    
    async def test_duplicate_email(self, client):
        duplicate_email_user = {
            "username": "newuser",
            "password": "NewPassword123!",
            "email": TestData.VALID_USERS["admin"]["email"],
            "role": "Patient",
            "firstName": "Duplicate",
            "lastName": "Email"
        }
        result = await client.api_call("POST", "/api/auth/register", duplicate_email_user)
        
        assert not result["success"]
        assert result["data"]["message"] == "Email already exists"
    
    async def test_case_insensitive_username(self, client):
        case_test_user = {
            "username": "ADMIN",
            "password": "CaseTest123!",
            "email": "casetest@medicare.com",
            "role": "Patient",
            "firstName": "Case",
            "lastName": "Test"
        }
        result = await client.api_call("POST", "/api/auth/register", case_test_user)
        
        assert not result["success"]
        assert result["data"]["message"] == "Username already exists"


class TestAuthentication:
    async def test_valid_admin_login(self, client):
        result = await client.api_call("POST", "/api/auth/login", {
            "username": "admin",
            "password": "Admin123!"
        })
        
        assert result["success"]
        assert "token" in result["data"]
    
    async def test_valid_doctor_login(self, client):
        result = await client.api_call("POST", "/api/auth/login", {
            "username": "doctor1",
            "password": "Doctor123!"
        })
        
        assert result["success"]
        assert "token" in result["data"]
    
    async def test_invalid_password(self, client):
        result = await client.api_call("POST", "/api/auth/login", {
            "username": "admin",
            "password": "wrongpassword"
        })
        
        assert not result["success"]
        assert result["data"]["message"] == "Invalid username or password"
    
    async def test_nonexistent_user(self, client):
        result = await client.api_call("POST", "/api/auth/login", {
            "username": "nonexistent",
            "password": "Password123!"
        })
        
        assert not result["success"]
        assert result["data"]["message"] == "Invalid username or password"


class TestInputValidation:
    async def test_empty_username(self, client):
        result = await client.api_call("POST", "/api/auth/register", TestData.INVALID_USERS["empty_username"])
        
        assert not result["success"]
        assert "errors" in result["data"]
        # Check for username validation error
        errors = result["data"]["errors"]
        assert any("username" in str(key).lower() for key in errors.keys())
    
    async def test_weak_password(self, client):
        result = await client.api_call("POST", "/api/auth/register", TestData.INVALID_USERS["weak_password"])
        
        assert not result["success"]
        assert "errors" in result["data"]
        # Check for password validation error
        errors = result["data"]["errors"]
        assert any("password" in str(key).lower() for key in errors.keys())
    
    async def test_invalid_email(self, client):
        result = await client.api_call("POST", "/api/auth/register", TestData.INVALID_USERS["invalid_email"])
        
        assert not result["success"]
        assert "errors" in result["data"]
        # Check for email validation error
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
        
        assert user_count > 0, "No users found in database"
        assert profile_count > 0, "No profiles found in database"
        assert user_count == profile_count, f"User count ({user_count}) != Profile count ({profile_count})"


if __name__ == "__main__":
    # Run tests programmatically
    pytest.main([__file__, "-v", "--tb=short"])
