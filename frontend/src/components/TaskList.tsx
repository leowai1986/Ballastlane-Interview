import { ClipboardList } from "lucide-react";
import type { TaskDto } from "../types";
import { TaskItemRow } from "./TaskItem";

export function TaskList({
  tasks,
  onEdit,
  onDelete,
}: {
  tasks: TaskDto[];
  onEdit: (task: TaskDto) => void;
  onDelete: (task: TaskDto) => void;
}) {
  if (tasks.length === 0) {
    return (
      <div className="card flex flex-col items-center justify-center gap-2 p-10 text-center">
        <div className="rounded-full bg-slate-100 p-3">
          <ClipboardList className="h-6 w-6 text-slate-600" />
        </div>
        <h3 className="mt-2 text-base font-semibold text-slate-900">No tasks yet</h3>
        <p className="max-w-md text-sm text-slate-600">Create your first task to start tracking work.</p>
      </div>
    );
  }

  return (
    <div className="grid gap-4">
      {tasks.map((t) => (
        <TaskItemRow key={t.id} task={t} onEdit={onEdit} onDelete={onDelete} />
      ))}
    </div>
  );
}

