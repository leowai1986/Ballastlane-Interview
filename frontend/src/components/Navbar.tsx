import { Link, NavLink } from "react-router-dom";
import { LogOut, Menu, X } from "lucide-react";
import { useState } from "react";
import { useAuth } from "../hooks/useAuth";

function NavItem({ to, label }: { to: string; label: string }) {
  return (
    <NavLink
      to={to}
      className={({ isActive }) =>
        [
          "rounded-lg px-3 py-2 text-sm font-medium",
          isActive ? "bg-slate-900 text-white" : "text-slate-700 hover:bg-slate-100",
        ].join(" ")
      }
    >
      {label}
    </NavLink>
  );
}

export function Navbar() {
  const { user, isAuthenticated, logout } = useAuth();
  const [open, setOpen] = useState(false);

  return (
    <header className="sticky top-0 z-40 border-b border-slate-200 bg-white/80 backdrop-blur">
      <div className="mx-auto flex w-full max-w-6xl items-center justify-between px-4 py-3 sm:px-6 lg:px-8">
        <div className="flex items-center gap-3">
          <Link to="/" className="text-base font-semibold text-slate-900">
            TaskManager
          </Link>
          <nav className="hidden items-center gap-2 md:flex">
            <NavItem to="/tasks" label="Tasks" />
          </nav>
        </div>

        <div className="flex items-center gap-2">
          {isAuthenticated ? (
            <>
              <span className="hidden text-sm text-slate-600 sm:block">
                {user?.name ? `Hola, ${user.name}` : "Authenticated"}
              </span>
              <button type="button" className="btn-secondary" onClick={logout}>
                <LogOut className="h-4 w-4" />
                <span className="hidden sm:inline">Logout</span>
              </button>
            </>
          ) : (
            <div className="hidden items-center gap-2 md:flex">
              <NavItem to="/login" label="Login" />
              <NavItem to="/register" label="Register" />
            </div>
          )}

          <button
            type="button"
            className="btn-secondary md:hidden"
            aria-label={open ? "Close menu" : "Open menu"}
            onClick={() => setOpen((v) => !v)}
          >
            {open ? <X className="h-4 w-4" /> : <Menu className="h-4 w-4" />}
          </button>
        </div>
      </div>

      {open ? (
        <div className="border-t border-slate-200 bg-white md:hidden">
          <div className="mx-auto w-full max-w-6xl px-4 py-3 sm:px-6 lg:px-8">
            <div className="flex flex-col gap-2">
              <NavItem to="/tasks" label="Tasks" />
              {!isAuthenticated ? (
                <>
                  <NavItem to="/login" label="Login" />
                  <NavItem to="/register" label="Register" />
                </>
              ) : null}
            </div>
          </div>
        </div>
      ) : null}
    </header>
  );
}

