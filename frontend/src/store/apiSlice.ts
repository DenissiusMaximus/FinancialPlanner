import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { setCredentials, logout } from './authSlice';
import { getToken } from '../utils/auth';

type AuthUserDto = {
  accessToken?: string | null;
  refreshToken?: string | null;
};

type SourceDtoLookup = {
  id: number;
  name?: string | null;
  amount?: number;
  isArchived?: boolean;
};

type SourceSummaryDto = {
  total?: number;
  sources?: SourceDtoLookup[] | null;
};

const baseQuery = fetchBaseQuery({
  baseUrl: 'http://localhost:5043', // Updated to match user backend port
  prepareHeaders: (headers) => {
    const token = getToken();
    if (token) {
      headers.set('authorization', `Bearer ${token}`);
    }
    return headers;
  },
});

const baseQueryWithReauth = async (args, api, extraOptions) => {
  let result = await baseQuery(args, api, extraOptions);

  if (result.error && result.error.status === 401) {
    // try to get a new token
    const refreshResult = await baseQuery(
      {
        url: '/api/Jwt/refreshToken',
        method: 'POST',
      },
      api,
      extraOptions
    );

    if (refreshResult.data) {
      const refreshData = refreshResult.data as string | { accessToken?: string; token?: string };
      const token =
        typeof refreshData === 'string'
          ? refreshData
          : refreshData.accessToken ?? refreshData.token;

      // store the new token
      if (token) {
        api.dispatch(setCredentials({ token }));
      }

      // retry the initial query
      result = await baseQuery(args, api, extraOptions);
    } else {
      api.dispatch(logout());
    }
  }
  return result;
};

export const apiSlice = createApi({
  reducerPath: 'api',
  baseQuery: baseQueryWithReauth,
  tagTypes: ['Aim', 'Category', 'Currency', 'Frequency', 'PlannedTransaction', 'Source', 'Transaction', 'TransactionType', 'User'],
  endpoints: (builder) => ({
    login: builder.mutation<AuthUserDto, { email: string; password: string }>({
      query: (credentials) => ({
        url: '/api/User/login',
        method: 'POST',
        body: credentials,
      }),
    }),
    register: builder.mutation<AuthUserDto, { name: string; email: string; password: string }>({
      query: (userData) => ({
        url: '/api/User/register',
        method: 'POST',
        body: userData,
      }),
    }),
    logout: builder.mutation({
      query: () => ({
        url: '/api/User/logout',
        method: 'POST',
      }),
    }),
    checkEmailAvailable: builder.query({
      query: (email) => ({
        url: '/api/User/email-available',
        params: { email },
      }),
    }),
    getUserMe: builder.query<AuthUserDto, void>({
      query: () => '/api/User/me',
      providesTags: ['User'],
    }),

    // Sources
    getSources: builder.query<SourceDtoLookup[], void>({
      query: () => '/api/Source',
      providesTags: ['Source'],
    }),
    getSourcesSummary: builder.query<SourceSummaryDto, void>({
      query: () => '/api/Source/summary',
      providesTags: ['Source'],
    }),
    createSource: builder.mutation<SourceDtoLookup, { name: string; amount: number; currencyId: number }>({
      query: (source) => ({
        url: '/api/Source',
        method: 'POST',
        body: source,
      }),
      invalidatesTags: ['Source'],
    }),
    updateSource: builder.mutation<SourceDtoLookup, { id: number; name?: string }>({
      query: ({ id, ...source }) => ({
        url: `/api/Source/${id}`,
        method: 'PATCH',
        body: source,
      }),
      invalidatesTags: ['Source'],
    }),
    deleteSource: builder.mutation({
      query: (id) => ({
        url: `/api/Source/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Source'],
    }),
    archiveSource: builder.mutation({
      query: (id) => ({
        url: `/api/Source/archive/${id}`,
        method: 'PATCH',
      }),
      invalidatesTags: ['Source'],
    }),
    unarchiveSource: builder.mutation({
      query: (id) => ({
        url: `/api/Source/unarchive/${id}`,
        method: 'PATCH',
      }),
      invalidatesTags: ['Source'],
    }),

    // Transactions
    getTransactions: builder.query({
      query: (params) => ({
        url: '/api/Transaction',
        params, // limit, offset, sourceIds, etc.
      }),
      providesTags: ['Transaction'],
    }),
    createTransaction: builder.mutation({
      query: (transaction) => ({
        url: '/api/Transaction',
        method: 'POST',
        body: transaction,
      }),
      invalidatesTags: ['Transaction', 'Source', 'Aim'],
    }),
    updateTransaction: builder.mutation({
      query: ({ id, ...transaction }) => ({
        url: `/api/Transaction/${id}`,
        method: 'PATCH',
        body: transaction,
      }),
      invalidatesTags: ['Transaction', 'Source', 'Aim'],
    }),
    deleteTransaction: builder.mutation({
      query: (id) => ({
        url: `/api/Transaction/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Transaction', 'Source', 'Aim'],
    }),
    
    // Transaction Types
    getTransactionTypes: builder.query<any, void>({
      query: () => '/api/TransactionType',
      providesTags: ['TransactionType'],
    }),

    // Planned Transactions
    getPlannedTransactions: builder.query<any, void>({
      query: () => '/api/PlannedTransaction',
      providesTags: ['PlannedTransaction'],
    }),
    createPlannedTransaction: builder.mutation({
      query: (plannedTx) => ({
        url: '/api/PlannedTransaction',
        method: 'POST',
        body: plannedTx,
      }),
      invalidatesTags: ['PlannedTransaction'],
    }),
    updatePlannedTransaction: builder.mutation({
      query: ({ id, ...plannedTx }) => ({
        url: `/api/PlannedTransaction/${id}`,
        method: 'PATCH',
        body: plannedTx,
      }),
      invalidatesTags: ['PlannedTransaction'],
    }),
    deletePlannedTransaction: builder.mutation({
      query: (id) => ({
        url: `/api/PlannedTransaction/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['PlannedTransaction'],
    }),

    // Frequencies
    getFrequencies: builder.query<any, void>({
      query: () => '/api/Frequency',
      providesTags: ['Frequency'],
    }),

    // Currencies
    getCurrencies: builder.query<any, void>({
      query: () => '/api/Currency',
      providesTags: ['Currency'],
    }),
    
    // Categories
    getCategories: builder.query<any, void>({
      query: () => '/api/Category',
      providesTags: ['Category'],
    }),
    createCategory: builder.mutation({
      query: (category) => ({
        url: '/api/Category',
        method: 'POST',
        body: category,
      }),
      invalidatesTags: ['Category'],
    }),
    updateCategory: builder.mutation({
      query: ({ id, ...category }) => ({
        url: `/api/Category/${id}`,
        method: 'PATCH',
        body: category,
      }),
      invalidatesTags: ['Category'],
    }),
    deleteCategory: builder.mutation({
      query: (id) => ({
        url: `/api/Category/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Category'],
    }),
    
    // Aims
    getAims: builder.query({
      query: (params) => ({
        url: '/api/Aim',
        params,
      }),
      providesTags: ['Aim'],
    }),
    createAim: builder.mutation({
      query: (aim) => ({
        url: '/api/Aim',
        method: 'POST',
        body: aim,
      }),
      invalidatesTags: ['Aim'],
    }),
    updateAim: builder.mutation({
      query: ({ id, ...aim }) => ({
        url: `/api/Aim/${id}`,
        method: 'PATCH',
        body: aim,
      }),
      invalidatesTags: ['Aim'],
    }),
    deleteAim: builder.mutation({
      query: (id) => ({
        url: `/api/Aim/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Aim'],
    }),
    addSourceToAim: builder.mutation({
      query: ({ aimId, sourceId }) => ({
        url: `/api/Aim/${aimId}/sources/${sourceId}`,
        method: 'POST',
      }),
      invalidatesTags: ['Aim', 'Source'],
    }),
    removeSourceFromAim: builder.mutation({
      query: ({ aimId, sourceId }) => ({
        url: `/api/Aim/${aimId}/sources/${sourceId}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Aim', 'Source'],
    }),
  }),
});

export const {
  useLoginMutation,
  useRegisterMutation,
  useLogoutMutation,
  useCheckEmailAvailableQuery,
  useGetUserMeQuery,
  useGetSourcesQuery,
  useGetSourcesSummaryQuery,
  useCreateSourceMutation,
  useUpdateSourceMutation,
  useDeleteSourceMutation,
  useArchiveSourceMutation,
  useUnarchiveSourceMutation,
  useGetTransactionsQuery,
  useCreateTransactionMutation,
  useUpdateTransactionMutation,
  useDeleteTransactionMutation,
  useGetTransactionTypesQuery,
  useGetPlannedTransactionsQuery,
  useCreatePlannedTransactionMutation,
  useUpdatePlannedTransactionMutation,
  useDeletePlannedTransactionMutation,
  useGetFrequenciesQuery,
  useGetCurrenciesQuery,
  useGetCategoriesQuery,
  useCreateCategoryMutation,
  useUpdateCategoryMutation,
  useDeleteCategoryMutation,
  useGetAimsQuery,
  useCreateAimMutation,
  useUpdateAimMutation,
  useDeleteAimMutation,
  useAddSourceToAimMutation,
  useRemoveSourceFromAimMutation,
} = apiSlice;
