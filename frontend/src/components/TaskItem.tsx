import { Calendar, Pencil, Trash2 } from "lucide-react";
import type { TaskDto, TaskStatus } from "../types";

function badge(status: TaskStatus) {
  switch (status) {
    case "Pending":
      return "bg-yellow-100 text-yellow-800 border-yellow-200";
    case "InProgress":
      return "bg-blue-100 text-blue-800 border-blue-200";
    case "Completed":
      return "bg-green-100 text-green-800 border-green-200";
  }
}

export function TaskItemRow({
  task,
  onEdit,
  onDelete,
}: {
  task: TaskDto;
  onEdit: (task: TaskDto) => void;
  onDelete: (task: TaskDto) => void;
}) {
  const due = task.dueDate ? new Date(task.dueDate).toLocaleDateString() : null;

  return (
    <div className="card p-4">
      <div className="flex items-start justify-between gap-4">
        <div className="min-w-0">
          <div className="flex flex-wrap items-center gap-2">
            <h3 className="truncate text-base font-semibold text-slate-900">{task.title}</h3>
            <span className={`inline-flex items-center rounded-full border px-2 py-0.5 text-xs ${badge(task.status)}`}>
              {task.status}
            </span>
          </div>
          {task.description ? <p className="mt-2 text-sm text-slate-600">{task.description}</p> : null}
          {due ? (
            <div className="mt-3 flex items-center gap-2 text-xs text-slate-500">
              <Calendar className="h-4 w-4" />
              <span>Due: {due}</span>
            </div>
          ) : null}
        </div>

        <div className="flex shrink-0 items-center gap-2">
          <button type="button" className="btn-secondary" onClick={() => onEdit(task)} aria-label="Edit task">
            <Pencil className="h-4 w-4" />
          </button>
          <button type="button" className="btn-danger" onClick={() => onDelete(task)} aria-label="Delete task">
            <Trash2 className="h-4 w-4" />
          </button>
        </div>
      </div>
    </div>
  );
}

