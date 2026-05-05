export const setToken = (token: string | null) => {
  if (token) {
    localStorage.setItem('accessToken', token);
  } else {
    localStorage.removeItem('accessToken');
  }
};

export const getToken = (): string | null => {
  return localStorage.getItem('accessToken');
};
