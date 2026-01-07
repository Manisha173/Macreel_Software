import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-layout',
  standalone: false,
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css']
})
export class LayoutComponent implements OnInit {

  sidebarOpen = true;
  isMobile = false;
  openMenu: string | null = null;

  ngOnInit(): void {
    this.checkScreen();
    window.addEventListener('resize', () => this.checkScreen());
  }

  checkScreen() {
    this.isMobile = window.innerWidth < 768;
    if (this.isMobile) {
      this.sidebarOpen = false;
    }
  }

  toggleSidebar() {
    this.sidebarOpen = !this.sidebarOpen;
  }

  toggleMenu(menu: string) {
    this.openMenu = this.openMenu === menu ? null : menu;
  }

  closeSidebarOnMobile() {
    if (this.isMobile) {
      this.sidebarOpen = false;
    }
  }
}
