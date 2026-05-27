import { useEffect, useState } from "react";
import { api } from "../services/api";

type HealthState =
  | { status: "loading" }
  | { status: "ok"; payload: unknown }
  | { status: "error"; message: string };

export function HealthPage() {
  const [state, setState] = useState<HealthState>({ status: "loading" });

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const { data } = await api.get("/api/health");
        if (!cancelled) setState({ status: "ok", payload: data });
      } catch {
        if (!cancelled) setState({ status: "error", message: "API is not reachable." });
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <div className="card p-6">
      <h1 className="text-xl font-semibold text-slate-900">API Health</h1>
      <p className="mt-1 text-sm text-slate-600">Simple public check against `GET /api/health`.</p>

      <div className="mt-4">
        {state.status === "loading" ? (
          <div className="flex items-center gap-3">
            <div className="h-5 w-5 animate-spin rounded-full border-2 border-slate-300 border-t-slate-900" />
            <span className="text-sm text-slate-700">Checking...</span>
          </div>
        ) : null}

        {state.status === "error" ? (
          <div className="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {state.message}
          </div>
        ) : null}

        {state.status === "ok" ? (
          <pre className="mt-3 overflow-auto rounded-lg bg-slate-900 p-4 text-xs text-slate-100">
            {JSON.stringify(state.payload, null, 2)}
          </pre>
        ) : null}
      </div>
    </div>
  );
}

