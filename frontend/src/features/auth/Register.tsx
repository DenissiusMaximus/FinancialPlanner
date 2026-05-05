import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import { useRegisterMutation, useCheckEmailAvailableQuery } from '../../store/apiSlice';
import { setCredentials } from '../../store/authSlice';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from '../../components/ui/Card';
import { Target, CheckCircle2, XCircle } from 'lucide-react';
import toast from 'react-hot-toast';

export function Register() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  
  // Debounce email check
  const [debouncedEmail, setDebouncedEmail] = useState('');
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedEmail(email);
    }, 500);
    return () => clearTimeout(timer);
  }, [email]);

  // Use skip if email is too short
  const { data: isEmailAvailable, isFetching: isCheckingEmail } = useCheckEmailAvailableQuery(debouncedEmail, {
    skip: debouncedEmail.length < 3 || !debouncedEmail.includes('@'),
  });

  const [register, { isLoading }] = useRegisterMutation();
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (isEmailAvailable === false) {
      toast.error('Email is already taken.');
      return;
    }

    try {
      const result = await register({ name, email, password }).unwrap();
      
      const token = typeof result === 'string' ? result : result.accessToken;
      
      if (token) {
        dispatch(setCredentials({ token }));
        toast.success('Registration successful!');
        navigate('/');
      } else {
        toast.error('Registration succeeded, but login failed. Please login manually.');
        navigate('/login');
      }
    } catch (err) {
      toast.error('Registration failed. Please try again.');
      console.error('Registration error:', err);
    }
  };

  const getEmailIndicator = () => {
    if (debouncedEmail.length < 3 || !debouncedEmail.includes('@')) return null;
    if (isCheckingEmail) return <span className="text-sm text-slate-500">Checking...</span>;
    if (isEmailAvailable) return <CheckCircle2 className="h-5 w-5 text-emerald-500" />;
    return <XCircle className="h-5 w-5 text-red-500" />;
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-50 p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-2 text-center">
          <div className="flex justify-center mb-4">
            <div className="rounded-full bg-primary-100 p-3">
              <Target className="h-8 w-8 text-primary-600" />
            </div>
          </div>
          <CardTitle className="text-2xl font-bold tracking-tight">Create an account</CardTitle>
          <CardDescription>Enter your details to get started</CardDescription>
        </CardHeader>
        <form onSubmit={handleSubmit}>
          <CardContent className="space-y-4">
            <Input
              label="Name"
              type="text"
              placeholder="John Doe"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              minLength={2}
            />
            
            <div className="relative">
              <Input
                label="Email"
                type="email"
                placeholder="name@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                className={isEmailAvailable === false ? "border-red-500 focus:ring-red-500 pr-10" : "pr-10"}
              />
              <div className="absolute right-3 top-[34px] flex items-center pointer-events-none">
                {getEmailIndicator()}
              </div>
              {isEmailAvailable === false && !isCheckingEmail && (
                <p className="mt-1 text-sm text-red-500">This email is already taken.</p>
              )}
            </div>

            <Input
              label="Password"
              type="password"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              minLength={6}
            />
          </CardContent>
          <CardFooter className="flex flex-col space-y-4">
            <Button 
              type="submit" 
              className="w-full" 
              isLoading={isLoading}
              disabled={isEmailAvailable === false || isCheckingEmail}
            >
              Sign up
            </Button>
            <div className="text-sm text-center text-slate-500">
              Already have an account?{' '}
              <Link to="/login" className="text-primary-600 hover:text-primary-500 font-medium">
                Sign in
              </Link>
            </div>
          </CardFooter>
        </form>
      </Card>
    </div>
  );
}
