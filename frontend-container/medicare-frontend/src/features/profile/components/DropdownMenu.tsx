import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "@shared/auth/AuthContext";

export const DropdownMenu: React.FC = () => {
  const navigate = useNavigate();
  const { logout, user } = useAuth();

  const handleLogout: React.MouseEventHandler<HTMLAnchorElement> = (e) => {
    e.preventDefault();
    try {
      logout();
    } finally {
      navigate("/login", { replace: true });
    }
  };

  return (
    <div
      className="absolute right-0 mt-2 w-40 bg-white rounded-lg shadow-lg py-2 z-50"
      role="menu"
      aria-label="User menu"
    >
      <Link
        to="/user/myprofile"
        className="block px-4 py-2 text-gray-700 hover:bg-blue-50"
        role="menuitem"
      >
        My Profile
      </Link>
      {user?.role === "Patient" && (
        <Link
          to="/user/wallet"
          className="block px-4 py-2 text-gray-700 hover:bg-blue-50"
          role="menuitem"
        >
          My Wallet
        </Link>
      )}
      <Link
        to="/login"
        onClick={handleLogout}
        className="block px-4 py-2 text-gray-700 hover:bg-blue-50"
        role="menuitem"
      >
        Logout
      </Link>
    </div>
  );
};
