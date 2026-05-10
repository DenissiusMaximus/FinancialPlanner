export default {
  financialPlanner: {
    input: './swagger.json',
    output: {
      target: './src/api/generated/endpoints.ts',
      schemas: './src/types/generated',
      client: 'react-query',
      httpClient: 'axios',
      override: {
        mutator: {
          path: './src/api/custom-instance.ts', 
          name: 'customInstance',               
        },
      },
    },
  },
};