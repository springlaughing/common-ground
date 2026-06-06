import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './styles/globals.css'
import App from './App'
import { MeReflection } from './pages/ReflectionPage/MeReflection'

// Minimal path-based routing: returning users open their saved `/me#TOKEN` link, which
// is its own entry point. Everything else is the questionnaire flow in <App />.
const isReflectionRoute = window.location.pathname === '/me'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    {isReflectionRoute ? <MeReflection /> : <App />}
  </StrictMode>,
)
