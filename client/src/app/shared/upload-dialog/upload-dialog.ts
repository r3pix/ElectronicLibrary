import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';
import { AssetsService } from '../../core/services/assets.service';
import { AssetType, UPLOADABLE_ASSET_TYPES } from '../../core/models/asset.model';

@Component({
  selector: 'app-upload-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressBarModule,
    MatSelectModule,
    TranslatePipe
  ],
  templateUrl: './upload-dialog.html',
  styleUrl: './upload-dialog.scss'
})
export class UploadDialog {
  private readonly fb = inject(FormBuilder);
  private readonly assetsService = inject(AssetsService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly dialogRef = inject(MatDialogRef<UploadDialog>);

  readonly typeOptions = UPLOADABLE_ASSET_TYPES;
  readonly form = this.fb.nonNullable.group({
    type: [AssetType.Score, Validators.required]
  });

  readonly selectedFile = signal<File | null>(null);
  readonly uploading = signal(false);

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0] ?? null);
  }

  cancel(): void {
    this.dialogRef.close(false);
  }

  submit(): void {
    const file = this.selectedFile();
    if (!file || this.form.invalid) {
      return;
    }

    this.uploading.set(true);
    const { type } = this.form.getRawValue();

    this.assetsService
      .requestUploadSas({ fileName: file.name, contentType: file.type || 'application/octet-stream', type })
      .subscribe({
        next: (sas) => this.putToBlob(sas.uploadUrl, file),
        error: () => {
          this.uploading.set(false);
          this.toastr.error(this.translate.instant('upload.toast.prepareFailed'));
        }
      });
  }

  private putToBlob(uploadUrl: string, file: File): void {
    this.assetsService
      .uploadToBlob(uploadUrl, file)
      .pipe(finalize(() => this.uploading.set(false)))
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('upload.toast.started'));
          this.dialogRef.close(true);
        },
        error: () => this.toastr.error(this.translate.instant('upload.toast.uploadFailed'))
      });
  }
}
