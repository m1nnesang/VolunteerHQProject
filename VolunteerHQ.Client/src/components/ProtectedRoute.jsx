import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function ProtectedRoute({ children, requireUnit = false }) {
  const { user } = useAuth()
  const location = useLocation()

  if (!user) {
    const loginPath = requireUnit ? '/units/login' : '/login'
    return <Navigate to={loginPath} state={{ from: location }} replace />
  }

  return children
}
