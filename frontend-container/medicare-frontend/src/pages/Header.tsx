import { useState, useEffect, useRef } from "react";
import { useNavigate } from 'react-router-dom';
import { Menu, X, User } from "lucide-react";
import DropdownMenu from "./Profile/DropdownMenu";
export default function Header() {
  const navigate = useNavigate();

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
      if (profileRef.current && !profileRef.current.contains(event.target as Node)) {
        setProfileOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const navItems = ["Appointments", "Doctors", "Patients", "Reports"];
  const pastelText = "text-blue-300";
  const pastelHover = "hover:text-blue-400";

  const handleProfileToggle = () => {
    if (drawerOpen) setDrawerOpen(false);
    setProfileOpen((o) => !o);
  };

  return (
    <>
      <header className="fixed top-0 left-0 w-full bg-blue-50 shadow-sm z-50 h-16">
        <div className="flex justify-between items-center h-full px-10">
          {/* Logo */}
          <div
            className="text-xl font-bold text-blue-600 cursor-pointer"
            onClick={() => navigate('/')}
          >
            IMUP Clinic
          </div>

          {/* Right side container: nav + profile/menu toggle */}
          <div className="flex items-center space-x-6">
            {/* Desktop navigation */}
            {!isMobile && (
              <nav className="flex space-x-6">
                {navItems.map((item) => (
                  <a
                    key={item}
                    className={`${pastelText} ${pastelHover} font-medium transition cursor-pointer`}
                  >
                    {item}
                  </a>
                ))}
              </nav>
            )}

            {/* Profile menu */}
            <div className="relative" ref={profileRef}>
              <button
                onClick={handleProfileToggle}
                className="p-2 rounded-full bg-blue-100 hover:bg-blue-200 transition"
              >
                <User size={20} className="text-blue-400" />
              </button>
              {profileOpen && (
                <div className="absolute right-0 mt-2 w-40 bg-white rounded-lg shadow-lg py-2 z-50">
                  <DropdownMenu/>
                </div>
              )}
            </div>

            {/* Mobile menu toggle */}
            {isMobile && (
              <button
                onClick={() => setDrawerOpen((o) => !o)}
                className="p-2 rounded-md transition-colors duration-200"
              >
                {drawerOpen ? (
                  <X size={24} className={`${pastelText} ${pastelHover}`} />
                ) : (
                  <Menu size={24} className={`${pastelText} ${pastelHover}`} />
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
            drawerOpen ? 'opacity-100' : 'opacity-0 pointer-events-none'
          }`}
          onClick={() => setDrawerOpen(false)}
        />
      )}

      {/* Mobile drawer */}
      {isMobile && (
        <aside
          className={`fixed top-16 right-0 h-[calc(100%-4rem)] w-2/3 max-w-xs bg-blue-100 shadow-lg z-50 p-6 flex flex-col space-y-4 transform transition-transform duration-300 ease-in-out ${
            drawerOpen ? 'translate-x-0' : 'translate-x-full'
          }`}
        >
          {navItems.map((item) => (
            <a
              key={item}
              className={`${pastelText} ${pastelHover} font-medium transition cursor-pointer`}
            >
              {item}
            </a>
          ))}
        </aside>
      )}
    </>
  );
}
