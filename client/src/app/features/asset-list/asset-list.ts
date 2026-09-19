import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { Subscription, filter, finalize, interval } from 'rxjs';
import { ConfirmDialog } from '../../shared/confirm-dialog/confirm-dialog';
import { ImagePreviewDialog } from '../../shared/image-preview-dialog/image-preview-dialog';
import { UploadDialog } from '../../shared/upload-dialog/upload-dialog';
import { AuthService } from '../../core/services/auth.service';
import { AssetsService } from '../../core/services/assets.service';
import {
  ALL_ASSET_TYPES,
  AssetModel,
  AssetStatus,
  AssetType,
  assetStatusLabelKey,
  assetTypeLabelKey
} from '../../core/models/asset.model';
import { formatBytes } from '../../core/utils/format.util';

const POLL_INTERVAL_MS = 4000;

@Component({
  selector: 'app-asset-list',
  imports: [
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    MatTableModule,
    MatTooltipModule,
    TranslatePipe
  ],
  templateUrl: './asset-list.html',
  styleUrl: './asset-list.scss'
})
export class AssetList implements OnInit, OnDestroy {
  private readonly assetsService = inject(AssetsService);
  private readonly authService = inject(AuthService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly dialog = inject(MatDialog);

  readonly AssetStatus = AssetStatus;
  readonly typeOptions = ALL_ASSET_TYPES;
  readonly displayedColumns = ['thumbnail', 'title', 'type', 'status', 'sizeBytes', 'uploadedBy', 'actions'];
  readonly assetTypeLabelKey = assetTypeLabelKey;
  readonly assetStatusLabelKey = assetStatusLabelKey;
  readonly formatBytes = formatBytes;

  readonly assets = signal<AssetModel[]>([]);
  readonly loading = signal(false);
  readonly selectedType = signal<AssetType | undefined>(undefined);
  readonly searchTerm = signal('');
  readonly isAdmin = this.authService.isAdmin;

  readonly filteredAssets = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) return this.assets();
    return this.assets().filter(
      (asset) => 
        (asset.title || asset.blobName).toLowerCase().includes(term) || 
        (asset.title || asset.blobName).toLowerCase().replace(' ', '').includes(term));
  });

  readonly hasPending = computed(() => this.assets().some((asset) => asset.status === AssetStatus.Pending));
  // Only the "nothing to show yet" case (first load, or a type switch before any data has arrived) —
  // a background refresh of an already-populated list keeps the existing table visible instead of
  // collapsing it, which is what caused the empty-table flash.
  readonly initialLoading = computed(() => this.loading() && this.assets().length === 0);

  private pollSubscription?: Subscription;

  ngOnInit(): void {
    this.refresh();
    this.pollSubscription = interval(POLL_INTERVAL_MS)
      .pipe(filter(() => this.hasPending()))
      .subscribe(() => this.refresh(true));
  }

  ngOnDestroy(): void {
    this.pollSubscription?.unsubscribe();
  }

  openThumbnailPreview(asset: AssetModel): void {
    if (!asset.thumbnailUrl) return;

    this.dialog.open(ImagePreviewDialog, {
      data: { imageUrl: asset.thumbnailUrl, title: asset.title || asset.blobName },
      maxWidth: '90vw',
      maxHeight: '90vh'
    });
  }

  openUploadDialog(): void {
    this.dialog
      .open(UploadDialog)
      .afterClosed()
      .subscribe((uploaded) => {
        if (uploaded) this.refresh();
      });
  }

  selectType(type: AssetType | undefined): void {
    if (this.selectedType() === type) return;
    this.selectedType.set(type);
    this.refresh();
  }

  onSearchInput(event: Event): void {
    this.searchTerm.set((event.target as HTMLInputElement).value);
  }

  refresh(silent = false): void {
    if (!silent) this.loading.set(true);
    this.assetsService
      .list(this.selectedType())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (assets) => this.assets.set(assets),
        error: () => this.toastr.error(this.translate.instant('assets.toast.loadFailed'))
      });
  }

  download(asset: AssetModel): void {
    // The Api sets Content-Disposition: attachment on this SAS (see GetAssetDownloadSasQueryHandler),
    // so the browser just downloads the file without navigating — no new-tab juggling needed, and
    // nothing here is subject to popup blocking.
    this.assetsService.getDownloadSas(asset.id).subscribe({
      next: (url) => {
        window.location.href = url;
      },
      error: () => this.toastr.error(this.translate.instant('assets.toast.downloadFailed'))
    });
  }

  confirmDelete(asset: AssetModel): void {
    this.dialog
      .open(ConfirmDialog, {
        data: {
          title: this.translate.instant('assets.confirmDelete.title'),
          message: this.translate.instant('assets.confirmDelete.message', {
            name: asset.title || asset.blobName
          })
        }
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (confirmed) this.delete(asset);
      });
  }

  private delete(asset: AssetModel): void {
    this.assetsService.delete(asset.id).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('assets.toast.deleted'));
        this.refresh();
      },
      error: () => this.toastr.error(this.translate.instant('assets.toast.deleteFailed'))
    });
  }
}
