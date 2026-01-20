import { useEffect, useRef, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { DropdownMenu } from "@features/profile/components";
import { useAuth } from "@shared/auth/AuthContext";
import {
  getDefaultDashboard,
  getNavigationForRole,
} from "@shared/constants/routes";
import { notificationsApi } from "@shared/services/notificationsApi";
import { Menu, User, X } from "lucide-react";

export default function Header() {
  const { user } = useAuth();
  const [isMobile, setIsMobile] = useState(false);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [profileOpen, setProfileOpen] = useState(false);
  const [unreadCount, setUnreadCount] = useState(0);
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

  const location = useLocation();

  useEffect(() => {
    let timer: number | undefined;
    const fetchUnread = async () => {
      if (!user?.id) {
        setUnreadCount(0);
        return;
      }
      try {
        const data = await notificationsApi.getForRecipient(user.id, true);
        setUnreadCount(data.length);
      } catch {
        // Silently fail for polling
      }
    };

    fetchUnread();

    const onUpdated = () => {
      fetchUnread();
    };
    window.addEventListener(
      "notifications:updated",
      onUpdated as (e: Event) => void
    );

    const onFocus = () => {
      fetchUnread();
    };
    const onVisibility = () => {
      if (document.visibilityState === "visible") fetchUnread();
    };
    window.addEventListener("focus", onFocus);
    document.addEventListener("visibilitychange", onVisibility);

    timer = window.setInterval(fetchUnread, 60000);

    return () => {
      if (timer) window.clearInterval(timer);
      window.removeEventListener(
        "notifications:updated",
        onUpdated as (e: Event) => void
      );
      window.removeEventListener("focus", onFocus);
      document.removeEventListener("visibilitychange", onVisibility);
    };
  }, [user?.id, location?.pathname]);

  const navItems = user ? getNavigationForRole(user.role) : [];

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
            to={user ? getDefaultDashboard(user.role) : "/"}
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
                  <Link key={item.label} to={item.path} className={linkClasses}>
                    {item.label}
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
                {unreadCount > 0 && (
                  <span
                    className="absolute -top-1 -right-1 inline-flex items-center justify-center rounded-full bg-red-500 text-white text-[10px] leading-none h-4 min-w-[16px] px-1 shadow"
                    aria-label={`${unreadCount} unread notifications`}
                  >
                    {unreadCount > 9 ? "9+" : unreadCount}
                  </span>
                )}
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
        <button
          type="button"
          className={`fixed inset-0 bg-blue-50 bg-opacity-80 z-40 transition-opacity duration-300 ease-in-out ${
            drawerOpen ? "opacity-100" : "opacity-0 pointer-events-none"
          }`}
          aria-label="Close menu"
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
              key={item.label}
              to={item.path}
              className={linkClasses}
              onClick={() => setDrawerOpen(false)}
            >
              {item.label}
            </Link>
          ))}
        </aside>
      )}
    </>
  );
}
