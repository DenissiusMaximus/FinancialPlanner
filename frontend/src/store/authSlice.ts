import { createSlice } from '@reduxjs/toolkit';
import { getToken, setToken } from '../utils/auth';

const initialState = {
  token: getToken(),
  isAuthenticated: !!getToken(),
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setCredentials: (state, action) => {
      state.token = action.payload.token;
      state.isAuthenticated = true;
      setToken(action.payload.token);
    },
    logout: (state) => {
      state.token = null;
      state.isAuthenticated = false;
      setToken(null);
    },
  },
});

export const { setCredentials, logout } = authSlice.actions;
export default authSlice.reducer;
