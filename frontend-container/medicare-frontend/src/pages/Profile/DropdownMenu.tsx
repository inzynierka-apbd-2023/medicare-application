import { Link } from "react-router-dom";

export default function DropdownMenu() {
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
      <Link
        to="/login"
        onClick={() => {}}
        className="block px-4 py-2 text-gray-700 hover:bg-blue-50"
        role="menuitem"
      >
        Logout
      </Link>
    </div>
  );
}
