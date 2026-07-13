import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AssetModel,
  AssetType,
  AssetUploadSasModel,
  CreateAssetUploadSasRequest
} from '../models/asset.model';
import { ApiResponse } from '../models/response.model';

@Injectable({ providedIn: 'root' })
export class AssetsService {
  private readonly http = inject(HttpClient);
  private readonly assetsUrl = `${environment.apiBaseUrl}/api/assets`;

  list(type?: AssetType): Observable<AssetModel[]> {
    const params: Record<string, string> = {};
    if (type !== undefined) params['type'] = String(type);

    return this.http
      .get<ApiResponse<AssetModel[]>>(this.assetsUrl, { params })
      .pipe(map((response) => response.result));
  }

  getDownloadSas(id: string): Observable<string> {
    return this.http
      .get<ApiResponse<string>>(`${this.assetsUrl}/${id}/download-sas`)
      .pipe(map((response) => response.result));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.assetsUrl}/${id}`);
  }

  requestUploadSas(request: CreateAssetUploadSasRequest): Observable<AssetUploadSasModel> {
    return this.http
      .post<ApiResponse<AssetUploadSasModel>>(`${environment.apiBaseUrl}/api/uploads/sas`, request)
      .pipe(map((response) => response.result));
  }

  // Direct-to-blob PUT (CLAUDE.md §4.1) — must not go through the auth interceptor, it's not our Api.
  uploadToBlob(uploadUrl: string, file: File): Observable<void> {
    return this.http.put<void>(uploadUrl, file, {
      headers: {
        'x-ms-blob-type': 'BlockBlob',
        'Content-Type': file.type || 'application/octet-stream'
      }
    });
  }
}
