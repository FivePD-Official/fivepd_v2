import { setupZoneTestEnv } from 'jest-preset-angular/setup-env/zone';

setupZoneTestEnv();

jest.mock('lodash-es/get', () => {
  return {
    __esModule: true,
    default: 'mockDefaultExport',
    nameExport: jest.fn(),
  }
});

jest.mock('@store/sidebar.actions', () => {
  return {
    __esModule: true,
    SidebarActions: {
      setRoutes: jest.fn(),
    }
  }
});

jest.mock('nanoid', () => {
  return {
    __esModule: true,
    nanoid: jest.fn().mockImplementation(() => 1)
  }
});
