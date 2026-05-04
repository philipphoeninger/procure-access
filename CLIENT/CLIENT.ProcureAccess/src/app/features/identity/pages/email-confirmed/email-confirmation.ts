import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { MatIconModule } from "@angular/material/icon";
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { map } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HttpResponse } from '@angular/common/http';
import { TranslatePipe } from '@ngx-translate/core';

enum EmailConfirmationState {
    confirming = 'confirming',
    success = 'success',
    failed = 'failed',
}

@Component({
  selector: 'pa-email-confirmation',
  imports: [
    MatButtonModule, 
    MatIconModule,
    RouterModule,
    MatProgressSpinnerModule,
    TranslatePipe
  ],
  templateUrl: './email-confirmation.html',
  styleUrl: './email-confirmation.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EmailConfirmation {
    private route = inject(ActivatedRoute);

    protected userId = signal("");
    protected token = signal("");

    protected EmailConfirmationState = EmailConfirmationState;
    protected confirmationState = signal(EmailConfirmationState.confirming);

    constructor(
        private router: Router,
        protected authService: AuthService,
        protected snackbarService: SnackbarService
    ) {
        this.route.queryParams.subscribe((params) => {
            const userId: string = params['userId'];
            const token: string = params['token'];
            if (userId == null || token == null) return; //gate
            this.userId.set(userId);
            this.token.set(token);
            this.sendEmailConfirmation();
        });
    }

    sendEmailConfirmation() {
        this.authService
            .confirmEmail(this.userId(), this.token())
            .pipe(
                map((response: HttpResponse<any>) => {
                if (response.ok) {
                    this.confirmationState.set(EmailConfirmationState.success);
                } else {
                    this.confirmationState.set(EmailConfirmationState.failed);
                }
                })
            )
            .subscribe();
    }
}
