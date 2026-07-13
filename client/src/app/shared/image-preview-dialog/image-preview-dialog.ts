import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

export interface ImagePreviewDialogData {
  imageUrl: string;
  title: string;
}

@Component({
  selector: 'app-image-preview-dialog',
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  templateUrl: './image-preview-dialog.html',
  styleUrl: './image-preview-dialog.scss'
})
export class ImagePreviewDialog {
  private readonly dialogRef = inject(MatDialogRef<ImagePreviewDialog>);
  readonly data = inject<ImagePreviewDialogData>(MAT_DIALOG_DATA);

  close(): void {
    this.dialogRef.close();
  }
}
