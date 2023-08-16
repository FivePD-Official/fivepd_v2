import { assertInInjectionContext, DestroyRef, inject } from "@angular/core";
import { ReplaySubject } from "rxjs";

export function injectDestroy() {
  assertInInjectionContext(injectDestroy);

  const destroyRef = inject(DestroyRef);
  const subject$ = new ReplaySubject<void>(1);

  destroyRef.onDestroy(() => {
    subject$.next();
    subject$.complete();
  });

  return subject$;
}
