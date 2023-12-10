import { createApp } from 'vue'
import './style.css'
import {createBrowserRouter, RouterProvider} from "react-router-dom"
import App from './App.vue'
import LoginPage from "./pages/Login"
import RegistrationPage from "./pages/Login"


const routes = [
  { path: '/', component: RegistrationPage },
  { path: '/registration', component: LoginPage },
]

const router = VueRouter.createRouter({
  history: VueRouter.createWebHashHistory(),
  routes,
})

const app = Vue.createApp({})

app.use(router)

app.mount('#app')
