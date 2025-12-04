import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TabStateService {
  private tabIndex = new BehaviorSubject<number>(0); // O valor padrão é 0 (primeira aba)

  setTabIndex(index: number) {
    this.tabIndex.next(index);
  }

  getTabIndex() {
    return this.tabIndex.asObservable();
  }
}
