export const TaskStatus = {
  Pending: 'Pending',
  InProgress: 'InProgress',
  Completed: 'Completed',
} as const;

export type TaskStatus = typeof TaskStatus[keyof typeof TaskStatus];

export const SortDirection = {
  Asc: 'Asc',
  Desc: 'Desc',
} as const;

export type SortDirection = typeof SortDirection[keyof typeof SortDirection];

export type TaskDto = {
  id: string;
  title: string;
  description?: string;
  status: TaskStatus;
  dueDate?: string;
  createdAt: string;
};

export interface TaskFilterRequest {
  page: number;
  pageSize: number;
  sortBy?: 'Title' | 'Status' | 'DueDate' | 'CreatedAt' | 'UpdatedAt';
  sortDirection: SortDirection;
  searchTerm?: string;
  status?: TaskStatus;
  dueDateFrom?: string;
  dueDateTo?: string;
};

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
};

export type CreateTaskRequest = {
  title: string;
  description: string;
  dueDate?: string;
};

export type UpdateTaskRequest = {
  title: string;
  description: string;
  status: TaskStatus;
  dueDate?: string;
};

export type AuthResponse = {
  token: string;
  email: string;
  name: string;
};

export type RegisterRequest = {
  email: string;
  password: string;
  name: string;
};

export type LoginRequest = {
  email: string;
  password: string;
};

