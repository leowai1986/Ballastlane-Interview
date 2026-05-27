import { useCallback, useEffect, useMemo, useState } from "react";
import type { CreateTaskRequest, TaskDto, UpdateTaskRequest } from "../types";
import { api } from "../services/api";

type TasksState = {
  tasks: TaskDto[];
  loading: boolean;
  error: string | null;
};

export function useTasks() {
  const [state, setState] = useState<TasksState>({ tasks: [], loading: false, error: null });

  const fetchTasks = useCallback(async () => {
    setState((s) => ({ ...s, loading: true, error: null }));
    try {
      const { data } = await api.get<TaskDto[]>("/api/tasks");
      setState({ tasks: data, loading: false, error: null });
    } catch (e) {
      setState((s) => ({ ...s, loading: false, error: "No se pudieron cargar las tareas." }));
      throw e;
    }
  }, []);

  const createTask = useCallback(async (req: CreateTaskRequest) => {
    setState((s) => ({ ...s, loading: true, error: null }));
    try {
      const { data } = await api.post<TaskDto>("/api/tasks", req);
      setState((s) => ({ tasks: [data, ...s.tasks], loading: false, error: null }));
      return data;
    } catch (e) {
      setState((s) => ({ ...s, loading: false, error: "No se pudo crear la tarea." }));
      throw e;
    }
  }, []);

  const updateTask = useCallback(async (id: string, req: UpdateTaskRequest) => {
    setState((s) => ({ ...s, loading: true, error: null }));
    try {
      await api.put(`/api/tasks/${id}`, req);
      setState((s) => ({
        tasks: s.tasks.map((t) => (t.id === id ? { ...t, ...req } : t)),
        loading: false,
        error: null,
      }));
    } catch (e) {
      setState((s) => ({ ...s, loading: false, error: "No se pudo actualizar la tarea." }));
      throw e;
    }
  }, []);

  const deleteTask = useCallback(async (id: string) => {
    setState((s) => ({ ...s, loading: true, error: null }));
    try {
      await api.delete(`/api/tasks/${id}`);
      setState((s) => ({ tasks: s.tasks.filter((t) => t.id !== id), loading: false, error: null }));
    } catch (e) {
      setState((s) => ({ ...s, loading: false, error: "No se pudo eliminar la tarea." }));
      throw e;
    }
  }, []);

  useEffect(() => {
    void fetchTasks();
  }, [fetchTasks]);

  return useMemo(
    () => ({
      ...state,
      fetchTasks,
      createTask,
      updateTask,
      deleteTask,
    }),
    [state, fetchTasks, createTask, updateTask, deleteTask]
  );
}

