import { X } from "lucide-react";
import { useMemo, useState, useEffect } from "react";
import type { CreateTaskRequest, TaskDto, TaskStatus, UpdateTaskRequest } from "../types";

type Mode = "create" | "edit";

type Props =
  | {
      open: boolean;
      mode: "create";
      initial?: undefined;
      onClose: () => void;
      onSubmit: (req: CreateTaskRequest) => Promise<void>;
    }
  | {
      open: boolean;
      mode: "edit";
      initial: TaskDto;
      onClose: () => void;
      onSubmit: (id: string, req: UpdateTaskRequest) => Promise<void>;
    };

type FieldErrors = Partial<Record<"title" | "status", string>>;

export function TaskForm(props: Props) {
  const { open, onClose } = props;
  const mode: Mode = props.mode;

  const initial = props.mode === "edit" ? props.initial : null;

  const [title, setTitle] = useState(initial ? initial.title : "");
  const [description, setDescription] = useState(initial ? initial.description ?? "" : "");
  const [status, setStatus] = useState<TaskStatus>(initial ? initial.status : "Pending");
  const [dueDate, setDueDate] = useState<string>(initial ? initial.dueDate?.slice(0, 10) ?? "" : "");
  const [errors, setErrors] = useState<FieldErrors>({});
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!open) return;

    if (props.mode === "edit" && props.initial) {
      setTitle(props.initial.title);
      setDescription(props.initial.description ?? "");
      setStatus(props.initial.status);
      setDueDate(props.initial.dueDate?.slice(0, 10) ?? "");
    } else {
      setTitle("");
      setDescription("");
      setStatus("Pending");
      setDueDate("");
    }
    setErrors({});
    setSubmitting(false);
  }, [open, props.mode, props.initial?.id]);

  const canSubmit = useMemo(() => title.trim() !== "", [title]);

  if (!open) return null;

  function validate(): FieldErrors {
    const next: FieldErrors = {};
    if (!title.trim()) next.title = "Title is required.";
    if (mode === "edit" && !status) next.status = "Status is required.";
    return next;
  }

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    const next = validate();
    setErrors(next);
    if (Object.keys(next).length > 0) return;

    setSubmitting(true);
    try {
      if (props.mode === "create") {
        const reqBase = {
          title: title.trim(),
          description: description.trim(),
        } satisfies Omit<CreateTaskRequest, "dueDate">;

        const req: CreateTaskRequest = dueDate
          ? { ...reqBase, dueDate: new Date(dueDate).toISOString() }
          : reqBase;

        await props.onSubmit(req);
      } else {
        const reqBase = {
          title: title.trim(),
          description: description.trim(),
          status,
        } satisfies Omit<UpdateTaskRequest, "dueDate">;

        const req: UpdateTaskRequest = dueDate
          ? { ...reqBase, dueDate: new Date(dueDate).toISOString() }
          : reqBase;

        await props.onSubmit(props.initial.id, req);
      }
      onClose();
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center bg-black/40 p-4 sm:items-center">
      <div className="card w-full max-w-lg p-0">
        <div className="flex items-center justify-between border-b border-slate-200 px-5 py-4">
          <h2 className="text-base font-semibold text-slate-900">
            {mode === "create" ? "New task" : "Edit task"}
          </h2>
          <button type="button" className="btn-secondary" onClick={onClose} aria-label="Close">
            <X className="h-4 w-4" />
          </button>
        </div>

        <form className="space-y-4 px-5 py-4" onSubmit={submit}>
          <div>
            <label className="label" htmlFor="title">
              Title <span className="text-rose-600">*</span>
            </label>
            <input id="title" className="input mt-1" value={title} onChange={(e) => setTitle(e.target.value)} />
            {errors.title ? <p className="mt-1 text-xs text-rose-600">{errors.title}</p> : null}
          </div>

          <div>
            <label className="label" htmlFor="description">
              Description
            </label>
            <textarea
              id="description"
              className="input mt-1 min-h-24"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <label className="label" htmlFor="status">
                Status {mode === "edit" ? <span className="text-rose-600">*</span> : null}
              </label>
              <select
                id="status"
                className="input mt-1"
                value={status}
                onChange={(e) => setStatus(e.target.value as TaskStatus)}
                disabled={mode === "create"}
              >
                <option value="Pending">Pending</option>
                <option value="InProgress">InProgress</option>
                <option value="Completed">Completed</option>
              </select>
              {errors.status ? <p className="mt-1 text-xs text-rose-600">{errors.status}</p> : null}
              {mode === "create" ? <p className="helper mt-1">New tasks start as Pending.</p> : null}
            </div>

            <div>
              <label className="label" htmlFor="dueDate">
                Due date
              </label>
              <input
                id="dueDate"
                className="input mt-1"
                type="date"
                value={dueDate}
                onChange={(e) => setDueDate(e.target.value)}
              />
            </div>
          </div>

          <div className="flex items-center justify-end gap-2 pt-2">
            <button type="button" className="btn-secondary" onClick={onClose} disabled={submitting}>
              Cancel
            </button>
            <button type="submit" className="btn-primary" disabled={!canSubmit || submitting}>
              {submitting ? "Saving..." : "Save"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}