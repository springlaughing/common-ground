import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './styles/globals.css'
import App from './App'
import { MeReflection } from './pages/ReflectionPage/MeReflection'
import { InvitePage } from './pages/InvitePage/InvitePage'

// Minimal path-based routing: returning users open their saved `/me#TOKEN` link and
// invitees open their `/invite#TOKEN` link — each is its own entry point. Everything
// else is the questionnaire flow in <App />.
const path = window.location.pathname
const route = path === '/me' ? 'me' : path === '/invite' ? 'invite' : 'app'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    {route === 'me' ? <MeReflection /> : route === 'invite' ? <InvitePage /> : <App />}
  </StrictMode>,
)
