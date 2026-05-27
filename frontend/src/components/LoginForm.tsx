import { useMemo, useState } from "react";
import type { LoginRequest } from "../types";
import { api } from "../services/api";
import { useAuth } from "../hooks/useAuth";

type FieldErrors = Partial<Record<keyof LoginRequest, string>>;

export function LoginForm() {
  const { login } = useAuth();

  const [form, setForm] = useState<LoginRequest>({
    email: "demo@ballastlane.com",
    password: "Pa$$w0rd!",
  });
  const [errors, setErrors] = useState<FieldErrors>({});
  const [submitting, setSubmitting] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);

  const canSubmit = useMemo(() => form.email.trim() !== "" && form.password.trim() !== "", [form]);

  function validate(values: LoginRequest): FieldErrors {
    const next: FieldErrors = {};
    if (!values.email.trim()) next.email = "Email is required.";
    if (!values.password.trim()) next.password = "Password is required.";
    return next;
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setApiError(null);
    const nextErrors = validate(form);
    setErrors(nextErrors);
    if (Object.keys(nextErrors).length > 0) return;

    setSubmitting(true);
    try {
      const { data } = await api.post("/api/auth/login", form);
      login(data);
      window.location.assign("/tasks");
    } catch {
      setApiError("Credenciales inválidas.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form className="card w-full max-w-md p-6" onSubmit={onSubmit}>
      <div className="mb-6">
        <h1 className="text-2xl font-semibold text-slate-900">Login</h1>
        <p className="mt-1 text-sm text-slate-600">Accede con tu cuenta para ver tus tareas.</p>
      </div>

      {apiError ? (
        <div className="mb-4 rounded-lg border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
          {apiError}
        </div>
      ) : null}

      <div className="space-y-4">
        <div>
          <label className="label" htmlFor="email">
            Email <span className="text-rose-600">*</span>
          </label>
          <input
            id="email"
            className="input mt-1"
            type="email"
            autoComplete="email"
            value={form.email}
            onChange={(e) => setForm((s) => ({ ...s, email: e.target.value }))}
            placeholder="demo@ballastlane.com"
          />
          {errors.email ? <p className="mt-1 text-xs text-rose-600">{errors.email}</p> : null}
        </div>

        <div>
          <label className="label" htmlFor="password">
            Password <span className="text-rose-600">*</span>
          </label>
          <input
            id="password"
            className="input mt-1"
            type="password"
            autoComplete="current-password"
            value={form.password}
            onChange={(e) => setForm((s) => ({ ...s, password: e.target.value }))}
            placeholder="Demo123!"
          />
          {errors.password ? <p className="mt-1 text-xs text-rose-600">{errors.password}</p> : null}
        </div>

        <button type="submit" className="btn-primary w-full" disabled={!canSubmit || submitting}>
          {submitting ? "Ingresando..." : "Ingresar"}
        </button>
      </div>
    </form>
  );
}

