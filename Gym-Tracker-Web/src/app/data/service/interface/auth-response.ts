export interface AuthResponse {
  token: string;
  userId: number; // Или другие данные, которые вы хотите хранить
  username: string;
  // Возможно, expirationDate, refreshToken и т.д.
}
