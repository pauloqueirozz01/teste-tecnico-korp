import { HttpInterceptorFn } from '@angular/common/http';

export const apiErrorInterceptor: HttpInterceptorFn = (request, next) => {
  return next(request);
};
