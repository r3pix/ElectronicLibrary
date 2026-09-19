import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { AssetModel, AssetStatus, AssetType, AssetUploadSasModel } from '../models/asset.model';
import { AssetsService } from './assets.service';

describe('AssetsService', () => {
  let service: AssetsService;
  let httpMock: HttpTestingController;
  const assetsUrl = `${environment.apiBaseUrl}/api/assets`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(AssetsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('list() omits the type param when no type is given', () => {
    service.list().subscribe();

    const req = httpMock.expectOne((r) => r.url === assetsUrl);
    expect(req.request.params.has('type')).toBe(false);
    req.flush({ result: [], code: 200, message: '', isError: false });
  });

  it('list() sends the numeric type as a query param', () => {
    service.list(AssetType.Score).subscribe();

    const req = httpMock.expectOne((r) => r.url === assetsUrl);
    expect(req.request.params.get('type')).toBe('0');
    req.flush({ result: [], code: 200, message: '', isError: false });
  });

  it('list() unwraps the ApiResponse envelope', () => {
    const assets: AssetModel[] = [
      {
        id: '1',
        type: AssetType.Score,
        status: AssetStatus.Ready,
        blobName: 'scores/a.pdf',
        title: 'a.pdf',
        contentType: 'application/pdf',
        sizeBytes: 10,
        uploadedBy: 'user@example.com',
        thumbnailUrl: null
      }
    ];
    let result: AssetModel[] | undefined;

    service.list().subscribe((r) => (result = r));

    httpMock
      .expectOne((r) => r.url === assetsUrl)
      .flush({ result: assets, code: 200, message: '', isError: false });

    expect(result).toEqual(assets);
  });

  it('getDownloadSas() unwraps the SAS URL string', () => {
    let result: string | undefined;

    service.getDownloadSas('abc').subscribe((r) => (result = r));

    httpMock
      .expectOne(`${assetsUrl}/abc/download-sas`)
      .flush({ result: 'https://blob.example/sas', code: 200, message: '', isError: false });

    expect(result).toBe('https://blob.example/sas');
  });

  it('delete() issues a DELETE to the asset id', () => {
    service.delete('abc').subscribe();

    const req = httpMock.expectOne(`${assetsUrl}/abc`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('requestUploadSas() posts to /api/uploads/sas and unwraps the result', () => {
    const sas: AssetUploadSasModel = { uploadUrl: 'https://blob.example/put?sig=abc', blobName: 'scores/abc.pdf' };
    let result: AssetUploadSasModel | undefined;

    service
      .requestUploadSas({ fileName: 'abc.pdf', contentType: 'application/pdf', type: AssetType.Score })
      .subscribe((r) => (result = r));

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/uploads/sas`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ fileName: 'abc.pdf', contentType: 'application/pdf', type: AssetType.Score });
    req.flush({ result: sas, code: 200, message: '', isError: false });

    expect(result).toEqual(sas);
  });

  it('uploadToBlob() PUTs directly to the given URL with the required blob headers', () => {
    const file = new File(['content'], 'a.pdf', { type: 'application/pdf' });

    service.uploadToBlob('https://blob.example/put?sig=abc', file).subscribe();

    const req = httpMock.expectOne('https://blob.example/put?sig=abc');
    expect(req.request.method).toBe('PUT');
    expect(req.request.headers.get('x-ms-blob-type')).toBe('BlockBlob');
    expect(req.request.headers.get('Content-Type')).toBe('application/pdf');
    req.flush(null);
  });
});
