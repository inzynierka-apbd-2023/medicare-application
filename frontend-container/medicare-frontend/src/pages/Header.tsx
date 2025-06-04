import { useState, useEffect, useRef } from "react";
import { Link } from "react-router-dom";
import { Menu, X, User } from "lucide-react";
import DropdownMenu from "./Profile/DropdownMenu";

export default function Header() {
  const [isMobile, setIsMobile] = useState(false);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [profileOpen, setProfileOpen] = useState(false);
  const profileRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const onResize = () => setIsMobile(window.innerWidth <= 600);
    onResize();
    window.addEventListener("resize", onResize);
    return () => window.removeEventListener("resize", onResize);
  }, []);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        profileRef.current &&
        !profileRef.current.contains(event.target as Node)
      ) {
        setProfileOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const navItems = [
    { name: "Appointments", path: "/appointments" },
    { name: "Doctors", path: "/doctors" },
    { name: "Patients", path: "/patients" },
    { name: "Documents", path: "/documents" },
  ];

  const linkClasses =
    "px-3 py-1 rounded-lg text-blue-700 hover:bg-blue-100 transition font-medium text-base";

  const handleProfileToggle = () => {
    if (drawerOpen) setDrawerOpen(false);
    setProfileOpen((o) => !o);
  };

  return (
    <>
      <header className="fixed top-0 left-0 w-full bg-blue-50 shadow-sm z-50 h-16">
        <div className="flex justify-between items-center h-full px-10">
          {/* Logo */}
          <Link
            to="/"
            className="text-xl font-bold text-blue-600 cursor-pointer"
          >
            IMUP Clinic
          </Link>

          {/* Right: nav + profile */}
          <div className="flex items-center space-x-6">
            {/* Desktop navigation */}
            {!isMobile && (
              <nav className="flex space-x-2">
                {navItems.map((item) => (
                  <Link key={item.name} to={item.path} className={linkClasses}>
                    {item.name}
                  </Link>
                ))}
              </nav>
            )}

            {/* Profile menu */}
            <div className="relative" ref={profileRef}>
              <button
                onClick={handleProfileToggle}
                className="p-2 rounded-full bg-blue-100 hover:bg-blue-200 transition"
                aria-label="Profile"
              >
                <User size={20} className="text-blue-400" />
              </button>
              {profileOpen && <DropdownMenu />}
            </div>

            {/* Mobile menu toggle */}
            {isMobile && (
              <button
                onClick={() => setDrawerOpen((o) => !o)}
                className="p-2 rounded-md transition-colors duration-200"
              >
                {drawerOpen ? (
                  <X size={24} className="text-blue-700" />
                ) : (
                  <Menu size={24} className="text-blue-700" />
                )}
              </button>
            )}
          </div>
        </div>
      </header>

      {/* Mobile backdrop */}
      {isMobile && (
        <div
          className={`fixed inset-0 bg-blue-50 bg-opacity-80 z-40 transition-opacity duration-300 ease-in-out ${
            drawerOpen ? "opacity-100" : "opacity-0 pointer-events-none"
          }`}
          onClick={() => setDrawerOpen(false)}
        />
      )}

      {/* Mobile drawer */}
      {isMobile && (
        <aside
          className={`fixed top-16 right-0 h-[calc(100%-4rem)] w-2/3 max-w-xs bg-blue-100 shadow-lg z-50 p-6 flex flex-col space-y-3 transform transition-transform duration-300 ease-in-out ${
            drawerOpen ? "translate-x-0" : "translate-x-full"
          }`}
        >
          {navItems.map((item) => (
            <Link
              key={item.name}
              to={item.path}
              className={linkClasses}
              onClick={() => setDrawerOpen(false)}
            >
              {item.name}
            </Link>
          ))}
        </aside>
      )}
    </>
  );
}
