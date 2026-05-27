import { useMemo, useState } from "react";
import type { RegisterRequest } from "../types";
import { api } from "../services/api";
import { useAuth } from "../hooks/useAuth";

type FieldErrors = Partial<Record<keyof RegisterRequest, string>>;

export function RegisterForm() {
  const { login } = useAuth();

  const [form, setForm] = useState<RegisterRequest>({
    name: "",
    email: "",
    password: "",
  });
  const [errors, setErrors] = useState<FieldErrors>({});
  const [submitting, setSubmitting] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);

  const canSubmit = useMemo(
    () => form.name.trim() !== "" && form.email.trim() !== "" && form.password.trim() !== "",
    [form]
  );

  function validate(values: RegisterRequest): FieldErrors {
    const next: FieldErrors = {};
    if (!values.name.trim()) next.name = "Name is required.";
    if (!values.email.trim()) next.email = "Email is required.";
    if (!values.password.trim()) next.password = "Password is required.";
    if (values.password.trim() && values.password.trim().length < 8) next.password = "Min 8 characters.";
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
      const { data } = await api.post("/api/auth/register", form);
      login(data);
      window.location.assign("/tasks");
    } catch {
      setApiError("No se pudo registrar. ¿Email ya existe?");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form className="card w-full max-w-md p-6" onSubmit={onSubmit}>
      <div className="mb-6">
        <h1 className="text-2xl font-semibold text-slate-900">Register</h1>
        <p className="mt-1 text-sm text-slate-600">Crea una cuenta para empezar a usar TaskManager.</p>
      </div>

      {apiError ? (
        <div className="mb-4 rounded-lg border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
          {apiError}
        </div>
      ) : null}

      <div className="space-y-4">
        <div>
          <label className="label" htmlFor="name">
            Name <span className="text-rose-600">*</span>
          </label>
          <input
            id="name"
            className="input mt-1"
            value={form.name}
            onChange={(e) => setForm((s) => ({ ...s, name: e.target.value }))}
            placeholder="Jane Doe"
          />
          {errors.name ? <p className="mt-1 text-xs text-rose-600">{errors.name}</p> : null}
        </div>

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
            placeholder="you@company.com"
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
            autoComplete="new-password"
            value={form.password}
            onChange={(e) => setForm((s) => ({ ...s, password: e.target.value }))}
            placeholder="At least 8 characters"
          />
          {errors.password ? <p className="mt-1 text-xs text-rose-600">{errors.password}</p> : null}
        </div>

        <button type="submit" className="btn-primary w-full" disabled={!canSubmit || submitting}>
          {submitting ? "Creando cuenta..." : "Crear cuenta"}
        </button>
      </div>
    </form>
  );
}

