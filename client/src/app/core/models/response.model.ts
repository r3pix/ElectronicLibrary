export interface ApiResponse<T> {
  result: T;
  code: number;
  message: string;
  isError: boolean;
}
