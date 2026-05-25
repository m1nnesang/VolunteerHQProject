import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getSubscriptions, subscribe, unsubscribe } from '../api/subscriptions'
import { useAuth } from '../context/AuthContext'

export default function SubscribeButton({ target, targetId }) {
  const { user, isUser } = useAuth()
  const [subscription, setSubscription] = useState(null)
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (!isUser) {
      setLoading(false)
      return
    }
    getSubscriptions(1, 100)
      .then(res => {
        const items = res.data?.items || []
        const existing = items.find(s => s.target === target && s.targetId === targetId)
        setSubscription(existing || null)
      })
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [target, targetId, isUser])

  const handleSubscribe = async () => {
    setSubmitting(true)
    try {
      const res = await subscribe(target, targetId)
      setSubscription(res.data)
    } catch (err) {
      console.error(err)
    } finally {
      setSubmitting(false)
    }
  }

  const handleUnsubscribe = async () => {
    if (!subscription) return
    setSubmitting(true)
    try {
      await unsubscribe(subscription.id)
      setSubscription(null)
    } catch (err) {
      console.error(err)
    } finally {
      setSubmitting(false)
    }
  }

  if (!user) {
    return (
      <Link
        to="/login"
        className="text-sm border border-[#2e303a] text-gray-400 px-4 py-2 rounded-lg hover:border-yellow-400 hover:text-yellow-400 transition-colors"
      >
        🔔 Підписатись
      </Link>
    )
  }

  if (!isUser) return null

  if (loading) {
    return (
      <div className="text-sm border border-[#2e303a] text-gray-600 px-4 py-2 rounded-lg">
        ...
      </div>
    )
  }

  if (subscription) {
    return (
      <button
        onClick={handleUnsubscribe}
        disabled={submitting}
        className="text-sm bg-yellow-400/10 border border-yellow-400/30 text-yellow-400 px-4 py-2 rounded-lg hover:bg-red-400/10 hover:border-red-400/30 hover:text-red-400 transition-colors disabled:opacity-50 group"
      >
        <span className="group-hover:hidden">✓ Підписано</span>
        <span className="hidden group-hover:inline">Відписатись</span>
      </button>
    )
  }

  return (
    <button
      onClick={handleSubscribe}
      disabled={submitting}
      className="text-sm border border-[#2e303a] text-gray-400 px-4 py-2 rounded-lg hover:border-yellow-400 hover:text-yellow-400 transition-colors disabled:opacity-50"
    >
      🔔 Підписатись
    </button>
  )
}
