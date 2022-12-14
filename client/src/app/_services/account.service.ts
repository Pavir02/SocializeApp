import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import {map} from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private currentUserSource = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUserSource.asObservable(); 

  constructor(private http:HttpClient) { }

register(model:any)
{
 return this.http.post<User>(this.baseUrl+"account/register", model).pipe(
  map(user=>
  {    
    if(user)
    {
      localStorage.setItem('user', JSON.stringify(user));
      this.currentUserSource.next(user);
      return user;
    }
  })
 )
}

  //store the user in the local storage after login and set the observable currentUserSource as well
  login(model:any)
  {
   return this.http.post<User>(this.baseUrl+"account/login", model).pipe(
    map((response:User)=>
    {
      const user = response;
      if(user)
      {
        localStorage.setItem('user', JSON.stringify(user));
        this.currentUserSource.next(user);
        return user;
      }
    })
   )
  }

  setCurrentUser(user:User)
  {
    this.currentUserSource.next(user);
  }

  //remove the user from the local storage and currentUserSource when logout
  logout()
  {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }

}
