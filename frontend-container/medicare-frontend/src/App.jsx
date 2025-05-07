import { useState } from 'react'
import './styles/App.css'
import Login from './pages/Login'
import Header from './pages/Header'
import PatientDashboard from './pages/Dashboard/PatientDashboard'

function App() {
  const [count, setCount] = useState(0)

  return (
    <div className="min-h-screen bg-gray-100">
      <PatientDashboard />
    </div>
  )
}

export default App
