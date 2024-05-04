import { JestConfigWithTsJest } from 'ts-jest';

const jestConfig: JestConfigWithTsJest = {
  preset: 'jest-preset-angular',
  setupFilesAfterEnv: ['<rootDir>/setup.jest.ts'],
  collectCoverageFrom: [
    '<rootDir>/src/app/**/*.ts',
    '!<rootDir>/src/app/**/index.ts',
    '!<rootDir>/src/app/**/*.module.ts'
  ],
  coverageDirectory: 'coverage',
  coverageReporters: [
    'lcov',
    'text-summary'
  ],
  testPathIgnorePatterns: [
    '<rootDir>/coverage/',
    '<rootDir>/dist/',
    '<rootDir>/e2e/',
    '<rootDir>/node_modules/',
    '<rootDir>/src/app/*.(js|scss)'
  ],
  testMatch: [
    '<rootDir>/src/app/*.spec.ts',
    '<rootDir>/src/app/**/*.spec.ts'
  ],
  moduleNameMapper: {
    "@helpers": ["<rootDir>/src/app/helpers"],
    "@enums": ["<rootDir>/src/app/enums"],
    "@services/(.*)": ["<rootDir>/src/app/services/$1"],
    "@models": ["<rootDir>/src/app/models"],
    "@store": ["<rootDir>/src/app/store"],
    "@store/(.*)$": ["<rootDir>/src/app/store/$1"],
    "@validators/(.*)$": ["<rootDir>/src/app/validators/$1"],
  }
};

export default jestConfig;
