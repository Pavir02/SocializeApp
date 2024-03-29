import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { HttpClient } from '@angular/common/http';
import { Photo } from '../_models/photo';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private http:HttpClient) { }

  getUsersWithRoles()
  {
    return this.http.get<User[]>(this.baseUrl + 'admin/users-with-roles');
  }

  updateUserRoles(username: string, roles:string[])
  {
    return this.http.post<string[]>(this.baseUrl + 'admin/edit-roles/' + username + '?roles=' + roles , {});
  }

  getPhotosForApproval()
  {
    return this.http.get<Photo[]>(this.baseUrl+ 'admin/photos-to-moderate');
  }

  approvePhoto(photoid : number)
  {
    return this.http.post<number>(this.baseUrl + 'admin/approve-photo/' + photoid, {});
  }

  rejectPhoto(photoid:number)
  {
    return this.http.post<number[]>(this.baseUrl+ 'admin/reject-photo/' + photoid, {});
  }

}
