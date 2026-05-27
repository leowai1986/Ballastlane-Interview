export type TaskStatus = "Pending" | "InProgress" | "Completed";

export type TaskDto = {
  id: string;
  title: string;
  description?: string;
  status: TaskStatus;
  dueDate?: string;
  createdAt: string;
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

