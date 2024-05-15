export function mockLodashGet() {
  jest.mock('lodash-es/get', () => {
    return {
      __esModule: true,
      default: 'mockDefaultExport',
      nameExport: jest.fn(),
    }
  });
}
