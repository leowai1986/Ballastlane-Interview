import { Plus } from "lucide-react";
import { useMemo, useState } from "react";
import type { CreateTaskRequest, TaskDto, UpdateTaskRequest } from "../types";
import { TaskForm } from "../components/TaskForm";
import { TaskList } from "../components/TaskList";
import { useTasks } from "../hooks/useTasks";

export function TasksPage() {
  const { tasks, loading, error, createTask, updateTask, deleteTask, fetchTasks } = useTasks();
  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<TaskDto | null>(null);

  const headerText = useMemo(() => {
    if (loading) return "Loading tasks...";
    return "Your tasks";
  }, [loading]);

  async function onCreate(req: CreateTaskRequest) {
    await createTask(req);
  }

  async function onEdit(id: string, req: UpdateTaskRequest) {
    await updateTask(id, req);
  }

  async function confirmDelete(task: TaskDto) {
    const ok = window.confirm(`Delete task "${task.title}"?`);
    if (!ok) return;
    await deleteTask(task.id);
  }

  return (
    <div className="space-y-5">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900">{headerText}</h1>
          <p className="mt-1 text-sm text-slate-600">Create, track, and complete tasks with a clean flow.</p>
        </div>

        <div className="flex items-center gap-2">
          <button type="button" className="btn-secondary" onClick={() => void fetchTasks()} disabled={loading}>
            Refresh
          </button>
          <button
            type="button"
            className="btn-primary"
            onClick={() => {
              setEditing(null);
              setFormOpen(true);
            }}
          >
            <Plus className="h-4 w-4" />
            New task
          </button>
        </div>
      </div>

      {error ? (
        <div className="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>
      ) : null}

      {loading ? (
        <div className="card flex items-center justify-center p-10">
          <div className="h-6 w-6 animate-spin rounded-full border-2 border-slate-300 border-t-slate-900" />
          <span className="ml-3 text-sm text-slate-600">Loading...</span>
        </div>
      ) : (
        <TaskList
          tasks={tasks}
          onEdit={(t) => {
            setEditing(t);
            setFormOpen(true);
          }}
          onDelete={(t) => void confirmDelete(t)}
        />
      )}

      {editing ? (
        <TaskForm
          open={formOpen}
          mode="edit"
          initial={editing}
          onClose={() => setFormOpen(false)}
          onSubmit={onEdit}
        />
      ) : (
        <TaskForm open={formOpen} mode="create" onClose={() => setFormOpen(false)} onSubmit={onCreate} />
      )}
    </div>
  );
}

