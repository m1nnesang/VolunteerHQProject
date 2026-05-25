import { useEffect, useRef, useState } from 'react'
import { getNotifications, markAsRead } from '../api/notifications'
import { connectNotifications } from '../api/realtime'

export default function NotificationBell() {
  const [open, setOpen] = useState(false)
  const [notifications, setNotifications] = useState([])
  const [loading, setLoading] = useState(false)
  const wrapperRef = useRef(null)

  const unreadCount = notifications.filter(n => !n.isRead).length

  const load = () => {
    setLoading(true)
    getNotifications(1, 20)
      .then(res => setNotifications(res.data?.items || []))
      .catch(console.error)
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    load()
    const interval = setInterval(load, 60000)
    return () => clearInterval(interval)
  }, [])

  useEffect(() => {
    const conn = connectNotifications((n) => {
      setNotifications(prev =>
        prev.some(x => x.id === n.id) ? prev : [n, ...prev]
      )
    })
    return () => conn?.stop?.()
  }, [])

  // Закриваємо дропдаун при кліку поза ним
  useEffect(() => {
    const handleClickOutside = (e) => {
      if (wrapperRef.current && !wrapperRef.current.contains(e.target)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  const handleClick = async (notification) => {
    if (!notification.isRead) {
      try {
        await markAsRead(notification.id)
        setNotifications(prev =>
          prev.map(n => n.id === notification.id ? { ...n, isRead: true } : n)
        )
      } catch (err) {
        console.error(err)
      }
    }
    setOpen(false)
  }

  return (
    <div ref={wrapperRef} className="relative">
      <button
        onClick={() => setOpen(o => !o)}
        className="relative w-9 h-9 flex items-center justify-center rounded-lg hover:bg-[#2e303a] transition-colors"
        aria-label="Сповіщення"
      >
        <svg className="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
            d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
        </svg>
        {unreadCount > 0 && (
          <span className="absolute top-1 right-1 w-4 h-4 bg-yellow-400 rounded-full text-black text-[10px] font-bold flex items-center justify-center">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>

      {open && (
        <div className="absolute right-0 top-12 w-80 max-h-96 bg-[#1c1d24] border border-[#2e303a] rounded-xl shadow-xl overflow-hidden z-50">
          <div className="px-4 py-3 border-b border-[#2e303a] flex items-center justify-between">
            <h3 className="text-white font-semibold text-sm">Сповіщення</h3>
            {unreadCount > 0 && (
              <span className="text-xs text-yellow-400">{unreadCount} нових</span>
            )}
          </div>

          <div className="overflow-y-auto max-h-80">
            {loading ? (
              <div className="p-4 text-center text-gray-500 text-sm">Завантаження...</div>
            ) : notifications.length === 0 ? (
              <div className="p-8 text-center text-gray-500 text-sm">
                Немає сповіщень
              </div>
            ) : (
              notifications.map(n => (
                <div
                  key={n.id}
                  onClick={() => handleClick(n)}
                  className={`px-4 py-3 border-b border-[#2e303a] ${
                    !n.isRead ? 'bg-yellow-400/5' : ''
                  }`}
                >
                  <div className="flex items-start gap-2">
                    {!n.isRead && (
                      <div className="w-2 h-2 bg-yellow-400 rounded-full mt-1.5 shrink-0"></div>
                    )}
                    <div className={`flex-1 min-w-0 ${n.isRead ? 'pl-4' : ''}`}>
                      <p className="text-sm text-gray-300 leading-snug">{n.text}</p>
                      <p className="text-xs text-gray-600 mt-1">
                        {new Date(n.sentAt).toLocaleString('uk-UA', {
                          day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit'
                        })}
                      </p>
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  )
}
