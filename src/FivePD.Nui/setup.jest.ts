import 'jest-preset-angular/setup-jest';

jest.mock('lodash-es/get', () => {
  return {
    __esModule: true,
    default: 'mockDefaultExport',
    nameExport: jest.fn(),
  }
});
