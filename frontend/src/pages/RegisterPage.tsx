import { Link } from "react-router-dom";
import { RegisterForm } from "../components/RegisterForm";

export function RegisterPage() {
  return (
    <div className="mx-auto flex w-full max-w-6xl items-center justify-center px-4 py-10">
      <div className="flex w-full flex-col items-center gap-4">
        <RegisterForm />
        <p className="text-sm text-slate-600">
          Ya tienes cuenta?{" "}
          <Link className="font-medium text-slate-900 underline" to="/login">
            Login
          </Link>
        </p>
      </div>
    </div>
  );
}

