import { inject, Injectable } from "@angular/core";
import { RouteOrDivider, RouteWithChildren, RouteWithoutChildren } from "@models";
import { SidebarActions } from "@store/sidebar.actions";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";

@Injectable({
  providedIn: 'root'
})
export class RouteHelper {

  private readonly _store = inject<Store<GlobalState>>(Store);

  isRoute(route: RouteOrDivider): route is RouteWithoutChildren {
    return !('divider' in route);
  }

  isDivider(route: RouteOrDivider): route is RouteWithoutChildren {
    return !this.isRoute(route);
  }

  hasChildren(route: RouteOrDivider): route is RouteWithChildren {
    return 'children' in route;
  }

  toggleRoute(route: RouteWithChildren): void {
    this._store.dispatch(SidebarActions.toggleRoute({route}));
  }
}
