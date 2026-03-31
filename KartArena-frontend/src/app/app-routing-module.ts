import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {myAuthData, myAuthGuard} from './core/guards/my-auth-guard';

const routes: Routes = [
  {
  path: 'admin',
  data: myAuthData({ requireAuth: false, requireAdmin: false }),
  loadChildren: () =>
    import('./modules/admin/admin-module').then(m => m.AdminModule)
  },
  
  {
    path: 'client',
    canActivate: [myAuthGuard],
    data: myAuthData({ requireAuth: true }),// bilo ko logiran
    loadChildren: () =>
      import('./modules/client/client-module').then(m => m.ClientModule)
  },
  {
    path: '',
    loadChildren: () =>
       import('./modules/public/public-module').then(m => m.PublicModule)
  },
  // fallback 404
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
