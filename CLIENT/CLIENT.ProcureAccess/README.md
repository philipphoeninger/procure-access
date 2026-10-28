# ProcureAccess

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 20.3.5.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

Unit and component tests run with [Vitest](https://vitest.dev) (Angular CLI builder `@angular/build:unit-test`, jsdom) and [Angular Testing Library](https://testing-library.com/docs/angular-testing-library/intro):

```bash
npx ng test
```

In a terminal the run stays in watch mode; without a terminal (CI) it runs once and exits. Every `TestBed` gets the providers from `src/test-providers.ts` (zoneless change detection).

`npm run test:coverage` always runs once (no watch mode, also in a terminal), prints a coverage summary and writes an lcov/HTML report to `coverage/procure-access/` (reported, not gated). How to write a component test: see `src/app/features/identity/dialogs/delete-account-dialog/delete-account-dialog.spec.ts` and the root `CLAUDE.md` §9.

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
