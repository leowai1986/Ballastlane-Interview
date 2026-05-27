import { Link, Navigate } from "react-router-dom";
import { LoginForm } from "../components/LoginForm";
import { useAuth } from "../hooks/useAuth";

export function LoginPage() {
  const { isAuthenticated } = useAuth();
  if (isAuthenticated) return <Navigate to="/tasks" replace />;

  return (
    <div className="mx-auto flex w-full max-w-6xl items-center justify-center px-4 py-10">
      <div className="flex w-full flex-col items-center gap-4">
        <LoginForm />
        <p className="text-sm text-slate-600">
          No tienes cuenta?{" "}
          <Link className="font-medium text-slate-900 underline" to="/register">
            Regístrate
          </Link>
        </p>
      </div>
    </div>
  );
}

