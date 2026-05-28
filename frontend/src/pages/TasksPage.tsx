import { Plus, Search } from "lucide-react";
import { useMemo, useState } from "react";
import { SortDirection, TaskStatus, type CreateTaskRequest, type TaskDto, type UpdateTaskRequest } from "../types";
import { TaskForm } from "../components/TaskForm";
import { TaskList } from "../components/TaskList";
import { useTasks } from "../hooks/useTasks";

export function TasksPage() {
  const {
    tasks,
    loading,
    error,
    filters,
    meta,
    setPage,
    setPageSize,
    setSort,
    setSearchTerm,
    setStatus,
    createTask,
    updateTask,
    deleteTask,
    fetchTasks,
  } = useTasks();

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<TaskDto | null>(null);
  const [searchInput, setSearchInput] = useState(filters.searchTerm ?? "");

  // Debounce simple para búsqueda
  const handleSearchChange = (value: string) => {
    setSearchInput(value);
    setSearchTerm(value);
  };

  const headerText = useMemo(() => {
    if (loading) return "Loading tasks...";
    return "Your tasks";
  }, [loading]);

  async function onCreate(req: CreateTaskRequest) {
    await createTask(req);
    setFormOpen(false);
  }

  async function onEdit(id: string, req: UpdateTaskRequest) {
    await updateTask(id, req);
    setEditing(null);
    setFormOpen(false);
  }

  async function confirmDelete(task: TaskDto) {
    const ok = window.confirm(`Delete task "${task.title}"?`);
    if (!ok) return;
    await deleteTask(task.id);
  }

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900">{headerText}</h1>
          <p className="mt-1 text-sm text-slate-600">
            {meta.totalCount} total • Page {meta.page} of {meta.totalPages}
          </p>
        </div>

        <div className="flex items-center gap-2">
          <button
            type="button"
            className="btn-secondary"
            onClick={() => void fetchTasks()}
            disabled={loading}
          >
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

      {/* Filters & Sorting */}
      <div className="card flex flex-col gap-3 sm:flex-row sm:items-end">
        <div className="flex-1">
          <label className="mb-1 block text-xs font-medium text-slate-600">Search</label>
          <div className="relative">
            <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-slate-400" />
            <input
              type="text"
              placeholder="Search by title..."
              value={searchInput}
              onChange={(e) => handleSearchChange(e.target.value)}
              className="w-full rounded-md border border-slate-300 py-2 pl-9 pr-3 text-sm focus:border-slate-900 focus:outline-none"
            />
          </div>
        </div>

        <div>
          <label className="mb-1 block text-xs font-medium text-slate-600">Status</label>
          <select
            value={filters.status ?? ""}
            onChange={(e) => setStatus(e.target.value as TaskStatus)}
            className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-slate-900 focus:outline-none"
          >
            <option value="">All</option>
            <option value={TaskStatus.Pending}>Pending</option>
            <option value={TaskStatus.InProgress}>In Progress</option>
            <option value={TaskStatus.Completed}>Completed</option>
          </select>
        </div>

        <div>
          <label className="mb-1 block text-xs font-medium text-slate-600">Sort by</label>
          <div className="flex gap-2">
            <select
              value={filters.sortBy ?? "CreatedAt"}
              onChange={(e) => setSort(e.target.value as typeof filters.sortBy)}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-slate-900 focus:outline-none"
            >
              <option value="CreatedAt">Created</option>
              <option value="Title">Title</option>
              <option value="Status">Status</option>
              <option value="DueDate">Due Date</option>
            </select>
            <button
              type="button"
              onClick={() =>
                setSort(filters.sortBy ?? "CreatedAt")
              }
              className="rounded-md border border-slate-300 px-3 py-2 text-sm hover:bg-slate-50"
              title="Toggle direction"
            >
              {filters.sortDirection === SortDirection.Asc ? "↑ Asc" : "↓ Desc"}
            </button>
          </div>
        </div>
      </div>

      {/* Error */}
      {error ? (
        <div className="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {error}
        </div>
      ) : null}

      {/* List */}
      {loading ? (
        <div className="card flex items-center justify-center p-10">
          <div className="h-6 w-6 animate-spin rounded-full border-2 border-slate-300 border-t-slate-900" />
          <span className="ml-3 text-sm text-slate-600">Loading...</span>
        </div>
      ) : tasks.length === 0 ? (
        <div className="card p-10 text-center text-sm text-slate-500">
          No tasks found. Try adjusting filters or create a new one.
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

      {/* Pagination */}
      {meta.totalPages > 1 && (
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <button
              className="btn-secondary"
              onClick={() => setPage(meta.page - 1)}
              disabled={!meta.hasPreviousPage || loading}
            >
              ← Previous
            </button>
            <span className="text-sm text-slate-600">
              Page {meta.page} of {meta.totalPages}
            </span>
            <button
              className="btn-secondary"
              onClick={() => setPage(meta.page + 1)}
              disabled={!meta.hasNextPage || loading}
            >
              Next →
            </button>
          </div>

          <div className="flex items-center gap-2">
            <label className="text-sm text-slate-600">Show</label>
            <select
              value={filters.pageSize}
              onChange={(e) => setPageSize(Number(e.target.value))}
              className="rounded-md border border-slate-300 px-2 py-1 text-sm"
            >
              <option value={10}>10</option>
              <option value={20}>20</option>
              <option value={50}>50</option>
            </select>
          </div>
        </div>
      )}

      {/* Modal */}
      {editing ? (
        <TaskForm
          open={formOpen}
          mode="edit"
          initial={editing}
          onClose={() => setFormOpen(false)}
          onSubmit={onEdit}
        />
      ) : (
        <TaskForm
          open={formOpen}
          mode="create"
          onClose={() => setFormOpen(false)}
          onSubmit={onCreate}
        />
      )}
    </div>
  );
}