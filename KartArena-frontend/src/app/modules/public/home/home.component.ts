import { Component } from '@angular/core';

type Slide = { src: string; alt: string };

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent {
  galleryIndex = 0;

  // NOTE: slike stavi u src/assets/ (npr. assets/Karting.jpg, assets/fia-2.jpg, assets/hero-2.jpg)
  slides: Slide[] = [
    { src: '/assets/karting.jpg', alt: 'KartArena - kartovi' },
    { src: '/assets/fia-2.jpg', alt: 'KartArena - trka' },
  ];

  prev() {
    this.galleryIndex = Math.max(0, this.galleryIndex - 1);
  }

  next() {
    this.galleryIndex = Math.min(this.slides.length - 1, this.galleryIndex + 1);
  }

  goTo(i: number) {
    this.galleryIndex = Math.max(0, Math.min(this.slides.length - 1, i));
  }


nextSlide() {
  this.galleryIndex = (this.galleryIndex + 1) % 2;
}

prevSlide() {
  this.galleryIndex = (this.galleryIndex - 1 + 2) % 2;
}

setSlide(i: number) {
  this.galleryIndex = i;
}


  onNewsletterSubmit(ev: Event) {
    ev.preventDefault();
    const form = ev.target as HTMLFormElement;
    const email = (form.elements.namedItem('email') as HTMLInputElement | null)?.value?.trim();

    if (!email) return;

    // TODO: ovdje pozovi svoj API (npr. NewsletterService)
    // Za sada samo reset.
    form.reset();
  }
}
