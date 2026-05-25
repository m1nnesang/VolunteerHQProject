import { createContext, useContext, useState, useEffect } from 'react'
import { getMyManagedOrg } from '../api/organizations'
import { stopRealtime } from '../api/realtime'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    try {
      const saved = localStorage.getItem('user')
      return saved ? JSON.parse(saved) : null
    } catch {
      return null
    }
  })

  const [managedOrgId, setManagedOrgIdState] = useState(() => {
    const saved = localStorage.getItem('managedOrgId')
    return saved ? parseInt(saved) : null
  })

  const setManagedOrg = (orgId) => {
    if (orgId) {
      localStorage.setItem('managedOrgId', String(orgId))
    } else {
      localStorage.removeItem('managedOrgId')
    }
    setManagedOrgIdState(orgId || null)
  }

  useEffect(() => {
    if (user?.type === 'user') {
      getMyManagedOrg()
        .then(res => setManagedOrg(res.data?.orgId ?? null))
        .catch(() => {})
    }
  }, [user])

  // Логін звичайного юзера
  const loginUser = (userData, token) => {
    const data = { ...userData, type: 'user' }
    localStorage.setItem('token', token)
    localStorage.setItem('user', JSON.stringify(data))
    setUser(data)
  }

  // Логін військової частини
  const loginUnit = (unitData, token) => {
    const data = { ...unitData, type: 'unit' }
    localStorage.setItem('token', token)
    localStorage.setItem('user', JSON.stringify(data))
    setUser(data)
  }

  const logoutUser = () => {
    stopRealtime()
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    localStorage.removeItem('managedOrgId')
    setUser(null)
    setManagedOrgIdState(null)
  }

  // Хелпери
  const isUnit = user?.type === 'unit'
  const isUser = user?.type === 'user'
  const isAdmin = user?.role === 'Admin'

  const displayName = user
    ? isUnit
      ? user.unitName
      : `${user.firstName} ${user.secondName?.charAt(0)}.`
    : null

  const initials = user
    ? isUnit
      ? user.unitName?.slice(0, 2).toUpperCase()
      : `${user.firstName?.charAt(0)}${user.secondName?.charAt(0)}`
    : null

  return (
    <AuthContext.Provider value={{
      user, loginUser, loginUnit, logoutUser,
      displayName, initials, isUnit, isUser, isAdmin,
      managedOrgId, setManagedOrg
    }}>
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = () => useContext(AuthContext)
