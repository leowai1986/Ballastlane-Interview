import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { SortDirection } from "../types";
import type {
  CreateTaskRequest,
  TaskDto,
  TaskFilterRequest,
  UpdateTaskRequest,
  PagedResult
} from "../types";
import { api } from "../services/api";

function buildPatch<T extends object>(obj: Record<string, unknown>): Partial<T> {
  return Object.fromEntries(
    Object.entries(obj).filter(([, v]) => v !== undefined)
  ) as Partial<T>;
}

const DEFAULT_FILTERS: TaskFilterRequest = {
  page: 1,
  pageSize: 10,
  sortBy: "CreatedAt",
  sortDirection: SortDirection.Desc,
};

type PaginationMeta = {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
};

type TasksState = {
  items: TaskDto[];
  filters: TaskFilterRequest;
  meta: PaginationMeta;
  loading: boolean;
  error: string | null;
};

export function useTasks() {
  const filtersRef = useRef<TaskFilterRequest>(DEFAULT_FILTERS);

  const [state, setState] = useState<TasksState>({
    items: [],
    filters: DEFAULT_FILTERS,
    meta: {
      page: 1,
      pageSize: 20,
      totalCount: 0,
      totalPages: 0,
      hasNextPage: false,
      hasPreviousPage: false,
    },
    loading: false,
    error: null,
  });

  const fetchTasks = useCallback(async () => {
    setState((s) => ({ ...s, loading: true, error: null }));
    try {
      const { data } = await api.get<PagedResult<TaskDto>>("/api/tasks", {
        params: filtersRef.current,
      });
      setState((s) => ({
        ...s,
        items: data.items,
        meta: {
          page: data.page,
          pageSize: data.pageSize,
          totalCount: data.totalCount,
          totalPages: data.totalPages,
          hasNextPage: data.hasNextPage,
          hasPreviousPage: data.hasPreviousPage,
        },
        loading: false,
        error: null,
      }));
    } catch (e) {
      setState((s) => ({
        ...s,
        loading: false,
        error: "No se pudieron cargar las tareas.",
      }));
      throw e;
    }
  }, []);

  const applyFilters = useCallback(
    (patch: Partial<TaskFilterRequest>, refetch = true) => {
      const next = { ...filtersRef.current, ...patch };
      // Si no estás cambiando la página explícitamente, volvé a la 1
      if (!("page" in patch)) {
        next.page = 1;
      }
      filtersRef.current = next;
      setState((s) => ({ ...s, filters: next }));
      if (refetch) {
        void fetchTasks();
      }
    },
    [fetchTasks]
  );

  const setPage = useCallback(
    (page: number) => applyFilters({ page }, true),
    [applyFilters]
  );

  const setPageSize = useCallback(
    (pageSize: number) => applyFilters({ pageSize, page: 1 }, true),
    [applyFilters]
  );

  const setSort = useCallback(
    (sortBy: TaskFilterRequest["sortBy"]) => {
      const current = filtersRef.current;
      const sortDirection: SortDirection =
        current.sortBy === sortBy && current.sortDirection === SortDirection.Asc
          ? SortDirection.Asc
          : SortDirection.Desc;

      applyFilters(
        buildPatch<TaskFilterRequest>({ sortBy, sortDirection, page: 1 }),
        true
      );
    },
    [applyFilters]
  );

  const setSearchTerm = useCallback(
    (searchTerm: string) =>
      applyFilters(
        buildPatch<TaskFilterRequest>({
          searchTerm: searchTerm || undefined,
          page: 1,
        }),
        true
      ),
    [applyFilters]
  );

  const setStatus = useCallback(
    (status: TaskFilterRequest["status"]) =>
      applyFilters(
        buildPatch<TaskFilterRequest>({ status, page: 1 }),
        true
      ),
    [applyFilters]
  );

  const setDateRange = useCallback(
    (from?: string, to?: string) =>
      applyFilters(
        buildPatch<TaskFilterRequest>({ dueDateFrom: from, dueDateTo: to, page: 1 }),
        true
      ),
    [applyFilters]
  );

  const createTask = useCallback(
    async (req: CreateTaskRequest) => {
      setState((s) => ({ ...s, loading: true, error: null }));
      try {
        const { data } = await api.post<TaskDto>("/api/tasks", req);
        await fetchTasks(); // refresca la página actual
        return data;
      } catch (e) {
        setState((s) => ({
          ...s,
          loading: false,
          error: "No se pudo crear la tarea.",
        }));
        throw e;
      }
    },
    [fetchTasks]
  );

  const updateTask = useCallback(
    async (id: string, req: UpdateTaskRequest) => {
      setState((s) => ({ ...s, loading: true, error: null }));
      try {
        await api.put(`/api/tasks/${id}`, req);
        await fetchTasks(); // refresca la página actual
      } catch (e) {
        setState((s) => ({
          ...s,
          loading: false,
          error: "No se pudo actualizar la tarea.",
        }));
        throw e;
      }
    },
    [fetchTasks]
  );

  const deleteTask = useCallback(
    async (id: string) => {
      setState((s) => ({ ...s, loading: true, error: null }));
      try {
        await api.delete(`/api/tasks/${id}`);
        await fetchTasks(); // refresca la página actual
      } catch (e) {
        setState((s) => ({
          ...s,
          loading: false,
          error: "No se pudo eliminar la tarea.",
        }));
        throw e;
      }
    },
    [fetchTasks]
  );

  useEffect(() => {
    void fetchTasks();
  }, [fetchTasks]);

  return useMemo(
    () => ({
      tasks: state.items,
      loading: state.loading,
      error: state.error,
      filters: state.filters,
      meta: state.meta,
      setPage,
      setPageSize,
      setSort,
      setSearchTerm,
      setStatus,
      setDateRange,
      fetchTasks,
      createTask,
      updateTask,
      deleteTask,
    }),
    [
      state,
      setPage,
      setPageSize,
      setSort,
      setSearchTerm,
      setStatus,
      setDateRange,
      fetchTasks,
      createTask,
      updateTask,
      deleteTask,
    ]
  );
}